using AutoMapper;
using MediatR;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Evidences.Dtos;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Surveys.Commands.UpdateTenureDocument;

/// <summary>
/// Handler for UpdateTenureDocumentCommand
/// Updates tenure document details and optionally replaces the file
/// </summary>
public class UpdateTenureDocumentCommandHandler : IRequestHandler<UpdateTenureDocumentCommand, EvidenceDto>
{
    private readonly ISurveyRepository _surveyRepository;
    private readonly IPersonPropertyRelationRepository _relationRepository;
    private readonly IPropertyUnitRepository _propertyUnitRepository;
    private readonly IEvidenceRepository _evidenceRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;
    private readonly IMapper _mapper;

    public UpdateTenureDocumentCommandHandler(
        ISurveyRepository surveyRepository,
        IPersonPropertyRelationRepository relationRepository,
        IPropertyUnitRepository propertyUnitRepository,
        IEvidenceRepository evidenceRepository,
        IFileStorageService fileStorageService,
        ICurrentUserService currentUserService,
        IAuditService auditService,
        IMapper mapper)
    {
        _surveyRepository = surveyRepository ?? throw new ArgumentNullException(nameof(surveyRepository));
        _relationRepository = relationRepository ?? throw new ArgumentNullException(nameof(relationRepository));
        _propertyUnitRepository = propertyUnitRepository ?? throw new ArgumentNullException(nameof(propertyUnitRepository));
        _evidenceRepository = evidenceRepository ?? throw new ArgumentNullException(nameof(evidenceRepository));
        _fileStorageService = fileStorageService ?? throw new ArgumentNullException(nameof(fileStorageService));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<EvidenceDto> Handle(UpdateTenureDocumentCommand request, CancellationToken cancellationToken)
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

        // Validate evidence belongs to survey's building context (via relation → property unit)
        if (evidence.PersonPropertyRelationId.HasValue)
        {
            var relation = await _relationRepository.GetByIdAsync(evidence.PersonPropertyRelationId.Value, cancellationToken);
            if (relation != null)
            {
                var propertyUnit = await _propertyUnitRepository.GetByIdAsync(relation.PropertyUnitId, cancellationToken);
                if (propertyUnit == null || propertyUnit.BuildingId != survey.BuildingId)
                    throw new ValidationException("Evidence does not belong to this survey's building");
            }
        }

        // Capture old values for audit
        var oldValues = System.Text.Json.JsonSerializer.Serialize(new
        {
            evidence.PersonPropertyRelationId,
            EvidenceType = evidence.EvidenceType.ToString(),
            evidence.OriginalFileName,
            evidence.Description,
            evidence.DocumentIssuedDate,
            evidence.DocumentExpiryDate,
            evidence.IssuingAuthority,
            evidence.DocumentReferenceNumber,
            evidence.Notes
        });

        // Update PersonPropertyRelationId if provided and different
        if (request.PersonPropertyRelationId.HasValue && request.PersonPropertyRelationId.Value != evidence.PersonPropertyRelationId)
        {
            var newRelation = await _relationRepository.GetByIdAsync(request.PersonPropertyRelationId.Value, cancellationToken)
                ?? throw new NotFoundException($"Person-property relation with ID {request.PersonPropertyRelationId} not found");

            // Verify new relation belongs to survey's building
            var newRelationUnit = await _propertyUnitRepository.GetByIdAsync(newRelation.PropertyUnitId, cancellationToken);
            if (newRelationUnit == null || newRelationUnit.BuildingId != survey.BuildingId)
                throw new ValidationException("Person-property relation does not belong to the survey's building");

            evidence.LinkToRelation(request.PersonPropertyRelationId.Value, currentUserId);
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
                    stream, request.File.FileName, "tenure-documents", request.SurveyId, cancellationToken);
            }

            var mimeType = _fileStorageService.GetMimeType(request.File.FileName);

            evidence.UpdateFileInfo(filePath, request.File.FileName, request.File.Length, mimeType, fileHash, currentUserId);
        }

        // Update evidence type if provided
        if (request.EvidenceType.HasValue)
        {
            evidence.UpdateEvidenceType((EvidenceType)request.EvidenceType.Value, currentUserId);
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
        if (request.PersonPropertyRelationId.HasValue) changedFields.Add("PersonPropertyRelationId");
        if (request.File != null) changedFields.Add("File");
        if (request.EvidenceType.HasValue) changedFields.Add("EvidenceType");
        if (request.Description != null) changedFields.Add("Description");
        if (request.DocumentIssuedDate.HasValue) changedFields.Add("DocumentIssuedDate");
        if (request.DocumentExpiryDate.HasValue) changedFields.Add("DocumentExpiryDate");
        if (request.IssuingAuthority != null) changedFields.Add("IssuingAuthority");
        if (request.DocumentReferenceNumber != null) changedFields.Add("DocumentReferenceNumber");
        if (request.Notes != null) changedFields.Add("Notes");

        // Audit logging
        await _auditService.LogActionAsync(
            actionType: AuditActionType.Update,
            actionDescription: $"Updated tenure document '{evidence.OriginalFileName}' in survey {survey.ReferenceCode}",
            entityType: "Evidence",
            entityId: evidence.Id,
            entityIdentifier: evidence.OriginalFileName,
            oldValues: oldValues,
            newValues: System.Text.Json.JsonSerializer.Serialize(new
            {
                evidence.PersonPropertyRelationId,
                EvidenceType = evidence.EvidenceType.ToString(),
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
