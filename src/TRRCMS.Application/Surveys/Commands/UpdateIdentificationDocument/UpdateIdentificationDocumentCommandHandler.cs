using AutoMapper;
using MediatR;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Evidences.Dtos;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Surveys.Commands.UpdateIdentificationDocument;

/// <summary>
/// Handler for UpdateIdentificationDocumentCommand
/// Updates identification document details and optionally replaces the file
/// </summary>
public class UpdateIdentificationDocumentCommandHandler : IRequestHandler<UpdateIdentificationDocumentCommand, EvidenceDto>
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

    public UpdateIdentificationDocumentCommandHandler(
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

    public async Task<EvidenceDto> Handle(UpdateIdentificationDocumentCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated");

        var survey = await _surveyRepository.GetByIdAsync(request.SurveyId, cancellationToken)
            ?? throw new NotFoundException($"Survey with ID {request.SurveyId} not found");

        if (survey.FieldCollectorId != currentUserId)
            throw new UnauthorizedAccessException("You can only update evidence for your own surveys");

        if (survey.Status != SurveyStatus.Draft)
            throw new ValidationException($"Cannot update evidence for survey in {survey.Status} status. Only Draft surveys can be modified.");

        // Get evidence
        var evidence = await _evidenceRepository.GetByIdAsync(request.EvidenceId, cancellationToken)
            ?? throw new NotFoundException($"Evidence with ID {request.EvidenceId} not found");

        // Validate evidence is linked to a person in this survey's building context
        if (evidence.PersonId.HasValue)
        {
            var person = await _personRepository.GetByIdAsync(evidence.PersonId.Value, cancellationToken);
            if (person?.HouseholdId != null)
            {
                var household = await _householdRepository.GetByIdAsync(person.HouseholdId.Value, cancellationToken);
                if (household != null)
                {
                    var propertyUnit = await _propertyUnitRepository.GetByIdAsync(household.PropertyUnitId, cancellationToken);
                    if (propertyUnit == null || propertyUnit.BuildingId != survey.BuildingId)
                        throw new ValidationException("Evidence does not belong to this survey's building");
                }
            }
        }

        // Capture old values for audit
        var oldValues = System.Text.Json.JsonSerializer.Serialize(new
        {
            evidence.PersonId,
            evidence.OriginalFileName,
            evidence.Description,
            evidence.DocumentIssuedDate,
            evidence.DocumentExpiryDate,
            evidence.IssuingAuthority,
            evidence.DocumentReferenceNumber,
            evidence.Notes
        });

        // Update PersonId if provided and different
        if (request.PersonId.HasValue && request.PersonId.Value != evidence.PersonId)
        {
            var newPerson = await _personRepository.GetByIdAsync(request.PersonId.Value, cancellationToken)
                ?? throw new NotFoundException($"Person with ID {request.PersonId} not found");
            evidence.LinkToPerson(request.PersonId.Value, currentUserId);
        }

        // Update file if provided
        if (request.File != null && request.File.Length > 0)
        {
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

            evidence.UpdateFileInfo(filePath, request.File.FileName, request.File.Length, mimeType, fileHash, currentUserId);
        }

        // Update description if provided
        if (request.Description != null)
        {
            evidence.UpdateDescription(request.Description, currentUserId);
        }

        // Update metadata (use existing values for fields not provided)
        evidence.UpdateMetadata(
            issuedDate: request.DocumentIssuedDate ?? evidence.DocumentIssuedDate,
            expiryDate: request.DocumentExpiryDate ?? evidence.DocumentExpiryDate,
            issuingAuthority: request.IssuingAuthority ?? evidence.IssuingAuthority,
            referenceNumber: request.DocumentReferenceNumber ?? evidence.DocumentReferenceNumber,
            notes: request.Notes ?? evidence.Notes,
            modifiedByUserId: currentUserId);

        // Save changes
        await _evidenceRepository.UpdateAsync(evidence, cancellationToken);
        await _evidenceRepository.SaveChangesAsync(cancellationToken);

        // Build changed fields
        var changedFields = new List<string>();
        if (request.PersonId.HasValue) changedFields.Add("PersonId");
        if (request.File != null) changedFields.Add("File");
        if (request.Description != null) changedFields.Add("Description");
        if (request.DocumentIssuedDate.HasValue) changedFields.Add("DocumentIssuedDate");
        if (request.DocumentExpiryDate.HasValue) changedFields.Add("DocumentExpiryDate");
        if (request.IssuingAuthority != null) changedFields.Add("IssuingAuthority");
        if (request.DocumentReferenceNumber != null) changedFields.Add("DocumentReferenceNumber");
        if (request.Notes != null) changedFields.Add("Notes");

        // Audit logging
        await _auditService.LogActionAsync(
            actionType: AuditActionType.Update,
            actionDescription: $"Updated identification document '{evidence.OriginalFileName}' in survey {survey.ReferenceCode}",
            entityType: "Evidence",
            entityId: evidence.Id,
            entityIdentifier: evidence.OriginalFileName,
            oldValues: oldValues,
            newValues: System.Text.Json.JsonSerializer.Serialize(new
            {
                evidence.PersonId,
                evidence.OriginalFileName,
                evidence.Description,
                evidence.DocumentIssuedDate,
                evidence.DocumentExpiryDate,
                evidence.IssuingAuthority,
                evidence.DocumentReferenceNumber,
                evidence.Notes
            }),
            changedFields: string.Join(", ", changedFields),
            cancellationToken: cancellationToken);

        var result = _mapper.Map<EvidenceDto>(evidence);
        result.IsExpired = evidence.IsExpired();

        return result;
    }
}
