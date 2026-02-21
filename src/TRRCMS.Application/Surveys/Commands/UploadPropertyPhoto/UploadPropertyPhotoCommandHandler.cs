using AutoMapper;
using MediatR;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Evidences.Dtos;
using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Surveys.Commands.UploadPropertyPhoto;

/// <summary>
/// Handler for UploadPropertyPhotoCommand
/// Uploads property photo and creates Evidence record
/// </summary>
public class UploadPropertyPhotoCommandHandler : IRequestHandler<UploadPropertyPhotoCommand, EvidenceDto>
{
    private readonly ISurveyRepository _surveyRepository;
    private readonly IPropertyUnitRepository _propertyUnitRepository;
    private readonly IPersonPropertyRelationRepository _relationRepository;
    private readonly IEvidenceRepository _evidenceRepository;
    private readonly IEvidenceRelationRepository _evidenceRelationRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;
    private readonly IMapper _mapper;

    public UploadPropertyPhotoCommandHandler(
        ISurveyRepository surveyRepository,
        IPropertyUnitRepository propertyUnitRepository,
        IPersonPropertyRelationRepository relationRepository,
        IEvidenceRepository evidenceRepository,
        IEvidenceRelationRepository evidenceRelationRepository,
        IFileStorageService fileStorageService,
        ICurrentUserService currentUserService,
        IAuditService auditService,
        IMapper mapper)
    {
        _surveyRepository = surveyRepository ?? throw new ArgumentNullException(nameof(surveyRepository));
        _propertyUnitRepository = propertyUnitRepository ?? throw new ArgumentNullException(nameof(propertyUnitRepository));
        _relationRepository = relationRepository ?? throw new ArgumentNullException(nameof(relationRepository));
        _evidenceRepository = evidenceRepository ?? throw new ArgumentNullException(nameof(evidenceRepository));
        _evidenceRelationRepository = evidenceRelationRepository ?? throw new ArgumentNullException(nameof(evidenceRelationRepository));
        _fileStorageService = fileStorageService ?? throw new ArgumentNullException(nameof(fileStorageService));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<EvidenceDto> Handle(UploadPropertyPhotoCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated");

        var survey = await _surveyRepository.GetByIdAsync(request.SurveyId, cancellationToken)
            ?? throw new NotFoundException($"Survey with ID {request.SurveyId} not found");

        if (survey.FieldCollectorId != currentUserId)
            throw new UnauthorizedAccessException("You can only upload evidence for your own surveys");

        if (survey.Status != SurveyStatus.Draft)
            throw new ValidationException($"Cannot upload evidence for survey in {survey.Status} status. Only Draft surveys can be modified.");

        if (request.PropertyUnitId.HasValue)
        {
            var propertyUnit = await _propertyUnitRepository.GetByIdAsync(request.PropertyUnitId.Value, cancellationToken)
                ?? throw new NotFoundException($"Property unit with ID {request.PropertyUnitId} not found");

            if (propertyUnit.BuildingId != survey.BuildingId)
                throw new ValidationException("Property unit does not belong to the survey's building");
        }

        PersonPropertyRelation? relation = null;
        if (request.PersonPropertyRelationId.HasValue)
        {
            relation = await _relationRepository.GetByIdAsync(request.PersonPropertyRelationId.Value, cancellationToken)
                ?? throw new NotFoundException($"Person-property relation with ID {request.PersonPropertyRelationId} not found");

            var relationUnit = await _propertyUnitRepository.GetByIdAsync(relation.PropertyUnitId, cancellationToken);
            if (relationUnit == null || relationUnit.BuildingId != survey.BuildingId)
                throw new ValidationException("Person-property relation does not belong to the survey's building");
        }

        if (request.File == null || request.File.Length == 0)
            throw new ValidationException("File is required");

        if (!_fileStorageService.ValidateFileSize(request.File.Length))
            throw new ValidationException("File size exceeds maximum allowed size");

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
        if (!_fileStorageService.ValidateFileExtension(request.File.FileName, allowedExtensions))
            throw new ValidationException($"Invalid file type. Allowed types: {string.Join(", ", allowedExtensions)}");

        string filePath;
        string fileHash;
        using (var stream = request.File.OpenReadStream())
        {
            fileHash = await _fileStorageService.CalculateFileHashAsync(stream, cancellationToken);
            filePath = await _fileStorageService.SaveFileAsync(
                stream, request.File.FileName, "property-photos", request.SurveyId, cancellationToken);
        }

        var mimeType = _fileStorageService.GetMimeType(request.File.FileName);

        // Create Evidence entity using EvidenceType.Photo enum
        var evidence = Evidence.Create(
            evidenceType: EvidenceType.Photo,
            description: request.Description,
            originalFileName: request.File.FileName,
            filePath: filePath,
            fileSizeBytes: request.File.Length,
            mimeType: mimeType,
            fileHash: fileHash,
            createdByUserId: currentUserId);

        if (!string.IsNullOrWhiteSpace(request.Notes))
        {
            evidence.UpdateMetadata(
                issuedDate: null,
                expiryDate: null,
                issuingAuthority: null,
                referenceNumber: null,
                notes: request.Notes,
                modifiedByUserId: currentUserId);
        }

        await _evidenceRepository.AddAsync(evidence, cancellationToken);
        await _evidenceRepository.SaveChangesAsync(cancellationToken);

        // Create EvidenceRelation join entity if linked to a person-property relation
        if (request.PersonPropertyRelationId.HasValue && relation != null)
        {
            var evidenceRelation = EvidenceRelation.Create(
                evidenceId: evidence.Id,
                personPropertyRelationId: request.PersonPropertyRelationId.Value,
                linkedBy: currentUserId);

            await _evidenceRelationRepository.AddAsync(evidenceRelation, cancellationToken);
            relation.SetHasEvidence(true, currentUserId);
            await _evidenceRelationRepository.SaveChangesAsync(cancellationToken);
        }

        await _auditService.LogActionAsync(
            actionType: AuditActionType.Create,
            actionDescription: $"Uploaded property photo '{request.File.FileName}' for survey {survey.ReferenceCode}",
            entityType: "Evidence",
            entityId: evidence.Id,
            entityIdentifier: request.File.FileName,
            oldValues: null,
            newValues: System.Text.Json.JsonSerializer.Serialize(new
            {
                EvidenceType = evidence.EvidenceType.ToString(),
                evidence.OriginalFileName,
                evidence.FileSizeBytes,
                evidence.MimeType,
                request.SurveyId,
                SurveyReferenceCode = survey.ReferenceCode,
                request.PropertyUnitId,
                request.PersonPropertyRelationId
            }),
            changedFields: "New Property Photo",
            cancellationToken: cancellationToken);

        // Re-fetch evidence with EvidenceRelations included
        var updatedEvidence = await _evidenceRepository.GetByIdAsync(evidence.Id, cancellationToken);
        var result = _mapper.Map<EvidenceDto>(updatedEvidence!);
        result.IsExpired = updatedEvidence!.IsExpired();

        return result;
    }
}
