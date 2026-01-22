using AutoMapper;
using MediatR;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Common.Services;
using TRRCMS.Application.Evidences.Dtos;
using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Surveys.Commands.UploadIdentificationDocument;

/// <summary>
/// Handler for UploadIdentificationDocumentCommand
/// Uploads ID document and links to person
/// </summary>
public class UploadIdentificationDocumentCommandHandler : IRequestHandler<UploadIdentificationDocumentCommand, EvidenceDto>
{
    private readonly ISurveyRepository _surveyRepository;
    private readonly IPersonRepository _personRepository;
    private readonly IHouseholdRepository _householdRepository;
    private readonly IPropertyUnitRepository _propertyUnitRepository;
    private readonly IEvidenceRepository _evidenceRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;
    private readonly IMapper _mapper;

    public UploadIdentificationDocumentCommandHandler(
        ISurveyRepository surveyRepository,
        IPersonRepository personRepository,
        IHouseholdRepository householdRepository,
        IPropertyUnitRepository propertyUnitRepository,
        IEvidenceRepository evidenceRepository,
        IFileStorageService fileStorageService,
        ICurrentUserService currentUserService,
        IAuditService auditService,
        IMapper mapper)
    {
        _surveyRepository = surveyRepository ?? throw new ArgumentNullException(nameof(surveyRepository));
        _personRepository = personRepository ?? throw new ArgumentNullException(nameof(personRepository));
        _householdRepository = householdRepository ?? throw new ArgumentNullException(nameof(householdRepository));
        _propertyUnitRepository = propertyUnitRepository ?? throw new ArgumentNullException(nameof(propertyUnitRepository));
        _evidenceRepository = evidenceRepository ?? throw new ArgumentNullException(nameof(evidenceRepository));
        _fileStorageService = fileStorageService ?? throw new ArgumentNullException(nameof(fileStorageService));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<EvidenceDto> Handle(UploadIdentificationDocumentCommand request, CancellationToken cancellationToken)
    {
        // Get current user
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated");

        // Get and validate survey
        var survey = await _surveyRepository.GetByIdAsync(request.SurveyId, cancellationToken);
        if (survey == null)
        {
            throw new NotFoundException($"Survey with ID {request.SurveyId} not found");
        }

        // Verify ownership
        if (survey.FieldCollectorId != currentUserId)
        {
            throw new UnauthorizedAccessException("You can only upload evidence for your own surveys");
        }

        // Verify survey is in Draft status
        if (survey.Status != SurveyStatus.Draft)
        {
            throw new ValidationException($"Cannot upload evidence for survey in {survey.Status} status. Only Draft surveys can be modified.");
        }

        // Get and validate person
        var person = await _personRepository.GetByIdAsync(request.PersonId, cancellationToken);
        if (person == null)
        {
            throw new NotFoundException($"Person with ID {request.PersonId} not found");
        }

        // Verify person belongs to survey's building (through household → property unit)
        if (person.HouseholdId.HasValue)
        {
            var household = await _householdRepository.GetByIdAsync(person.HouseholdId.Value, cancellationToken);
            if (household != null)
            {
                var propertyUnit = await _propertyUnitRepository.GetByIdAsync(household.PropertyUnitId, cancellationToken);
                if (propertyUnit == null || propertyUnit.BuildingId != survey.BuildingId)
                {
                    throw new ValidationException("Person does not belong to the survey's building");
                }
            }
        }

        // Validate file
        if (request.File == null || request.File.Length == 0)
        {
            throw new ValidationException("File is required");
        }

        // Validate file size
        if (!_fileStorageService.ValidateFileSize(request.File.Length))
        {
            throw new ValidationException("File size exceeds maximum allowed size");
        }

        // Validate file extension (documents and images)
        var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png" };
        if (!_fileStorageService.ValidateFileExtension(request.File.FileName, allowedExtensions))
        {
            throw new ValidationException($"Invalid file type. Allowed types: {string.Join(", ", allowedExtensions)}");
        }

        // Save file to storage
        string filePath;
        string fileHash;
        using (var stream = request.File.OpenReadStream())
        {
            // Calculate hash first
            fileHash = await _fileStorageService.CalculateFileHashAsync(stream, cancellationToken);

            // Save file
            filePath = await _fileStorageService.SaveFileAsync(
                stream,
                request.File.FileName,
                "identification-documents",
                request.SurveyId,
                cancellationToken);
        }

        // Get MIME type
        var mimeType = _fileStorageService.GetMimeType(request.File.FileName);

        // Create Evidence entity
        var evidence = Evidence.Create(
            evidenceType: "IdentificationDocument",
            description: request.Description,
            originalFileName: request.File.FileName,
            filePath: filePath,
            fileSizeBytes: request.File.Length,
            mimeType: mimeType,
            fileHash: fileHash,
            createdByUserId: currentUserId
        );

        // Link to person
        evidence.LinkToPerson(request.PersonId, currentUserId);

        // Update metadata
        evidence.UpdateMetadata(
            issuedDate: request.DocumentIssuedDate,
            expiryDate: request.DocumentExpiryDate,
            issuingAuthority: request.IssuingAuthority,
            referenceNumber: request.DocumentReferenceNumber,
            notes: request.Notes,
            modifiedByUserId: currentUserId
        );

        // Mark person as having identification document
        person.MarkIdentificationDocumentUploaded(currentUserId);

        // Save evidence
        await _evidenceRepository.AddAsync(evidence, cancellationToken);
        await _personRepository.UpdateAsync(person, cancellationToken);
        await _evidenceRepository.SaveChangesAsync(cancellationToken);

        // Audit logging
        await _auditService.LogActionAsync(
            actionType: AuditActionType.Create,
            actionDescription: $"Uploaded identification document '{request.File.FileName}' for person {person.GetFullNameArabic()} in survey {survey.ReferenceCode}",
            entityType: "Evidence",
            entityId: evidence.Id,
            entityIdentifier: request.File.FileName,
            oldValues: null,
            newValues: System.Text.Json.JsonSerializer.Serialize(new
            {
                evidence.EvidenceType,
                evidence.OriginalFileName,
                evidence.FileSizeBytes,
                request.PersonId,
                PersonName = person.GetFullNameArabic(),
                request.SurveyId,
                SurveyReferenceCode = survey.ReferenceCode
            }),
            changedFields: "New Identification Document",
            cancellationToken: cancellationToken
        );

        // Map to DTO
        var result = _mapper.Map<EvidenceDto>(evidence);
        result.IsExpired = evidence.IsExpired();

        return result;
    }
}