using AutoMapper;
using MediatR;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
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
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated");

        var survey = await _surveyRepository.GetByIdAsync(request.SurveyId, cancellationToken)
            ?? throw new NotFoundException($"Survey with ID {request.SurveyId} not found");

        if (survey.FieldCollectorId != currentUserId)
            throw new UnauthorizedAccessException("You can only upload evidence for your own surveys");

        if (survey.Status != SurveyStatus.Draft)
            throw new ValidationException($"Cannot upload evidence for survey in {survey.Status} status. Only Draft surveys can be modified.");

        var person = await _personRepository.GetByIdAsync(request.PersonId, cancellationToken)
            ?? throw new NotFoundException($"Person with ID {request.PersonId} not found");

        if (person.HouseholdId.HasValue)
        {
            var household = await _householdRepository.GetByIdAsync(person.HouseholdId.Value, cancellationToken);
            if (household != null)
            {
                var propertyUnit = await _propertyUnitRepository.GetByIdAsync(household.PropertyUnitId, cancellationToken);
                if (propertyUnit == null || propertyUnit.BuildingId != survey.BuildingId)
                    throw new ValidationException("Person does not belong to the survey's building");
            }
        }

        if (request.File == null || request.File.Length == 0)
            throw new ValidationException("File is required");

        if (!_fileStorageService.ValidateFileSize(request.File.Length))
            throw new ValidationException("File size exceeds maximum allowed size");

        var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png" };
        if (!_fileStorageService.ValidateFileExtension(request.File.FileName, allowedExtensions))
            throw new ValidationException($"Invalid file type. Allowed types: {string.Join(", ", allowedExtensions)}");

        string filePath;
        string fileHash;
        using (var stream = request.File.OpenReadStream())
        {
            fileHash = await _fileStorageService.CalculateFileHashAsync(stream, cancellationToken);
            filePath = await _fileStorageService.SaveFileAsync(
                stream, request.File.FileName, "identification-documents", request.SurveyId, cancellationToken);
        }

        var mimeType = _fileStorageService.GetMimeType(request.File.FileName);

        // Create Evidence entity using EvidenceType.IdentificationDocument enum
        var evidence = Evidence.Create(
            evidenceType: EvidenceType.IdentificationDocument,
            description: request.Description,
            originalFileName: request.File.FileName,
            filePath: filePath,
            fileSizeBytes: request.File.Length,
            mimeType: mimeType,
            fileHash: fileHash,
            createdByUserId: currentUserId);

        evidence.LinkToPerson(request.PersonId, currentUserId);

        evidence.UpdateMetadata(
            issuedDate: request.DocumentIssuedDate,
            expiryDate: request.DocumentExpiryDate,
            issuingAuthority: request.IssuingAuthority,
            referenceNumber: request.DocumentReferenceNumber,
            notes: request.Notes,
            modifiedByUserId: currentUserId);

        person.MarkIdentificationDocumentUploaded(currentUserId);

        await _evidenceRepository.AddAsync(evidence, cancellationToken);
        await _personRepository.UpdateAsync(person, cancellationToken);
        await _evidenceRepository.SaveChangesAsync(cancellationToken);

        await _auditService.LogActionAsync(
            actionType: AuditActionType.Create,
            actionDescription: $"Uploaded identification document '{request.File.FileName}' for person {person.GetFullNameArabic()} in survey {survey.ReferenceCode}",
            entityType: "Evidence",
            entityId: evidence.Id,
            entityIdentifier: request.File.FileName,
            oldValues: null,
            newValues: System.Text.Json.JsonSerializer.Serialize(new
            {
                EvidenceType = evidence.EvidenceType.ToString(),
                evidence.OriginalFileName,
                evidence.FileSizeBytes,
                request.PersonId,
                PersonName = person.GetFullNameArabic(),
                request.SurveyId,
                SurveyReferenceCode = survey.ReferenceCode
            }),
            changedFields: "New Identification Document",
            cancellationToken: cancellationToken);

        var result = _mapper.Map<EvidenceDto>(evidence);
        result.IsExpired = evidence.IsExpired();

        return result;
    }
}
