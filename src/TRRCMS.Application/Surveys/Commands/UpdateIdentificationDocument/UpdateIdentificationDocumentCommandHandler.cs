using AutoMapper;
using MediatR;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.IdentificationDocuments.Dtos;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Surveys.Commands.UpdateIdentificationDocument;

/// <summary>
/// Handler for UpdateIdentificationDocumentCommand
/// Updates identification document details and optionally replaces the file (in-place, no versioning)
/// </summary>
public class UpdateIdentificationDocumentCommandHandler : IRequestHandler<UpdateIdentificationDocumentCommand, IdentificationDocumentDto>
{
    private readonly ISurveyRepository _surveyRepository;
    private readonly IPersonRepository _personRepository;
    private readonly IHouseholdRepository _householdRepository;
    private readonly IPropertyUnitRepository _propertyUnitRepository;
    private readonly IIdentificationDocumentRepository _idDocRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;
    private readonly IMapper _mapper;

    public UpdateIdentificationDocumentCommandHandler(
        ISurveyRepository surveyRepository,
        IPersonRepository personRepository,
        IHouseholdRepository householdRepository,
        IPropertyUnitRepository propertyUnitRepository,
        IIdentificationDocumentRepository idDocRepository,
        IFileStorageService fileStorageService,
        ICurrentUserService currentUserService,
        IAuditService auditService,
        IMapper mapper)
    {
        _surveyRepository = surveyRepository ?? throw new ArgumentNullException(nameof(surveyRepository));
        _personRepository = personRepository ?? throw new ArgumentNullException(nameof(personRepository));
        _householdRepository = householdRepository ?? throw new ArgumentNullException(nameof(householdRepository));
        _propertyUnitRepository = propertyUnitRepository ?? throw new ArgumentNullException(nameof(propertyUnitRepository));
        _idDocRepository = idDocRepository ?? throw new ArgumentNullException(nameof(idDocRepository));
        _fileStorageService = fileStorageService ?? throw new ArgumentNullException(nameof(fileStorageService));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<IdentificationDocumentDto> Handle(UpdateIdentificationDocumentCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated");

        var survey = await _surveyRepository.GetByIdAsync(request.SurveyId, cancellationToken)
            ?? throw new NotFoundException($"Survey with ID {request.SurveyId} not found");

        if (survey.FieldCollectorId != currentUserId)
        {
            var currentUser = await _currentUserService.GetCurrentUserAsync(cancellationToken);
            if (currentUser == null || !currentUser.HasPermission(Permission.Surveys_EditAll))
                throw new UnauthorizedAccessException("You can only update documents for your own surveys");
        }

        if (survey.Status != SurveyStatus.Draft)
            throw new ValidationException($"Cannot update documents for survey in {survey.Status} status. Only Draft surveys can be modified.");

        var idDoc = await _idDocRepository.GetByIdAsync(request.EvidenceId, cancellationToken)
            ?? throw new NotFoundException($"Identification document with ID {request.EvidenceId} not found");

        // Validate document belongs to a person in this survey's building context
        var person = await _personRepository.GetByIdAsync(idDoc.PersonId, cancellationToken);
        if (person?.HouseholdId != null)
        {
            var household = await _householdRepository.GetByIdAsync(person.HouseholdId.Value, cancellationToken);
            if (household != null)
            {
                var propertyUnit = await _propertyUnitRepository.GetByIdAsync(household.PropertyUnitId, cancellationToken);
                if (propertyUnit == null || propertyUnit.BuildingId != survey.BuildingId)
                    throw new ValidationException("Document does not belong to this survey's building");
            }
        }

        // Capture old values for audit
        var oldValues = System.Text.Json.JsonSerializer.Serialize(new
        {
            idDoc.PersonId,
            idDoc.OriginalFileName,
            idDoc.Description,
            idDoc.DocumentIssuedDate,
            idDoc.DocumentExpiryDate,
            idDoc.IssuingAuthority,
            idDoc.DocumentReferenceNumber,
            idDoc.Notes
        });

        // Update PersonId if provided and different
        if (request.PersonId.HasValue && request.PersonId.Value != idDoc.PersonId)
        {
            var newPerson = await _personRepository.GetByIdAsync(request.PersonId.Value, cancellationToken)
                ?? throw new NotFoundException($"Person with ID {request.PersonId} not found");
            idDoc.LinkToPerson(request.PersonId.Value, currentUserId);
        }

        // Update file if provided (in-place, no versioning)
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
                if (!await _fileStorageService.ValidateFileMagicBytesAsync(stream, request.File.FileName))
                    throw new ValidationException("File content does not match the declared file type. Ensure the file is a valid PDF or image.");

                fileHash = await _fileStorageService.CalculateFileHashAsync(stream, cancellationToken);
                filePath = await _fileStorageService.SaveFileAsync(
                    stream, request.File.FileName, "identification-documents", request.SurveyId, cancellationToken);
            }

            var mimeType = _fileStorageService.GetMimeType(request.File.FileName);
            idDoc.UpdateFileInfo(filePath, request.File.FileName, request.File.Length, mimeType, fileHash, currentUserId);
        }

        if (request.DocumentType.HasValue)
            idDoc.UpdateDocumentType((DocumentType)request.DocumentType.Value, currentUserId);

        if (request.Description != null)
            idDoc.UpdateDescription(request.Description, currentUserId);

        idDoc.UpdateMetadata(
            issuedDate: request.DocumentIssuedDate ?? idDoc.DocumentIssuedDate,
            expiryDate: request.DocumentExpiryDate ?? idDoc.DocumentExpiryDate,
            issuingAuthority: request.IssuingAuthority ?? idDoc.IssuingAuthority,
            referenceNumber: request.DocumentReferenceNumber ?? idDoc.DocumentReferenceNumber,
            notes: request.Notes ?? idDoc.Notes,
            modifiedByUserId: currentUserId);

        await _idDocRepository.UpdateAsync(idDoc, cancellationToken);
        await _idDocRepository.SaveChangesAsync(cancellationToken);

        var changedFields = new List<string>();
        if (request.PersonId.HasValue) changedFields.Add("PersonId");
        if (request.File != null) changedFields.Add("File");
        if (request.DocumentType.HasValue) changedFields.Add("DocumentType");
        if (request.Description != null) changedFields.Add("Description");
        if (request.DocumentIssuedDate.HasValue) changedFields.Add("DocumentIssuedDate");
        if (request.DocumentExpiryDate.HasValue) changedFields.Add("DocumentExpiryDate");
        if (request.IssuingAuthority != null) changedFields.Add("IssuingAuthority");
        if (request.DocumentReferenceNumber != null) changedFields.Add("DocumentReferenceNumber");
        if (request.Notes != null) changedFields.Add("Notes");

        await _auditService.LogActionAsync(
            actionType: AuditActionType.Update,
            actionDescription: $"Updated identification document '{idDoc.OriginalFileName}' in survey {survey.ReferenceCode}",
            entityType: "IdentificationDocument",
            entityId: idDoc.Id,
            entityIdentifier: idDoc.OriginalFileName,
            oldValues: oldValues,
            newValues: System.Text.Json.JsonSerializer.Serialize(new
            {
                idDoc.PersonId,
                idDoc.OriginalFileName,
                idDoc.Description,
                idDoc.DocumentIssuedDate,
                idDoc.DocumentExpiryDate,
                idDoc.IssuingAuthority,
                idDoc.DocumentReferenceNumber,
                idDoc.Notes
            }),
            changedFields: string.Join(", ", changedFields),
            cancellationToken: cancellationToken);

        return _mapper.Map<IdentificationDocumentDto>(idDoc);
    }
}
