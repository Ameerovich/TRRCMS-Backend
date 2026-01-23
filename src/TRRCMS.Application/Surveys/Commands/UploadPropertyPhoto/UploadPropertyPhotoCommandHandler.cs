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
    private readonly IFileStorageService _fileStorageService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;
    private readonly IMapper _mapper;

    public UploadPropertyPhotoCommandHandler(
        ISurveyRepository surveyRepository,
        IPropertyUnitRepository propertyUnitRepository,
        IPersonPropertyRelationRepository relationRepository,
        IEvidenceRepository evidenceRepository,
        IFileStorageService fileStorageService,
        ICurrentUserService currentUserService,
        IAuditService auditService,
        IMapper mapper)
    {
        _surveyRepository = surveyRepository ?? throw new ArgumentNullException(nameof(surveyRepository));
        _propertyUnitRepository = propertyUnitRepository ?? throw new ArgumentNullException(nameof(propertyUnitRepository));
        _relationRepository = relationRepository ?? throw new ArgumentNullException(nameof(relationRepository));
        _evidenceRepository = evidenceRepository ?? throw new ArgumentNullException(nameof(evidenceRepository));
        _fileStorageService = fileStorageService ?? throw new ArgumentNullException(nameof(fileStorageService));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<EvidenceDto> Handle(UploadPropertyPhotoCommand request, CancellationToken cancellationToken)
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

        // Validate property unit if provided
        if (request.PropertyUnitId.HasValue)
        {
            var propertyUnit = await _propertyUnitRepository.GetByIdAsync(request.PropertyUnitId.Value, cancellationToken);
            if (propertyUnit == null)
            {
                throw new NotFoundException($"Property unit with ID {request.PropertyUnitId} not found");
            }

            // Verify property unit belongs to survey's building
            if (propertyUnit.BuildingId != survey.BuildingId)
            {
                throw new ValidationException("Property unit does not belong to the survey's building");
            }
        }

        // Validate person-property relation if provided
        if (request.PersonPropertyRelationId.HasValue)
        {
            var relation = await _relationRepository.GetByIdAsync(request.PersonPropertyRelationId.Value, cancellationToken);
            if (relation == null)
            {
                throw new NotFoundException($"Person-property relation with ID {request.PersonPropertyRelationId} not found");
            }

            // Verify relation's property unit belongs to survey's building
            var relationUnit = await _propertyUnitRepository.GetByIdAsync(relation.PropertyUnitId, cancellationToken);
            if (relationUnit == null || relationUnit.BuildingId != survey.BuildingId)
            {
                throw new ValidationException("Person-property relation does not belong to the survey's building");
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

        // Validate file extension (images only)
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
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
                "property-photos",
                request.SurveyId,
                cancellationToken);
        }

        // Get MIME type
        var mimeType = _fileStorageService.GetMimeType(request.File.FileName);

        // Create Evidence entity
        var evidence = Evidence.Create(
            evidenceType: "PropertyPhoto",
            description: request.Description,
            originalFileName: request.File.FileName,
            filePath: filePath,
            fileSizeBytes: request.File.Length,
            mimeType: mimeType,
            fileHash: fileHash,
            createdByUserId: currentUserId
        );

        // Link to person-property relation if provided
        if (request.PersonPropertyRelationId.HasValue)
        {
            evidence.LinkToRelation(request.PersonPropertyRelationId.Value, currentUserId);
        }

        // Add notes if provided
        if (!string.IsNullOrWhiteSpace(request.Notes))
        {
            evidence.UpdateMetadata(
                issuedDate: null,
                expiryDate: null,
                issuingAuthority: null,
                referenceNumber: null,
                notes: request.Notes,
                modifiedByUserId: currentUserId
            );
        }

        // Save evidence
        await _evidenceRepository.AddAsync(evidence, cancellationToken);
        await _evidenceRepository.SaveChangesAsync(cancellationToken);

        // Audit logging
        await _auditService.LogActionAsync(
            actionType: AuditActionType.Create,
            actionDescription: $"Uploaded property photo '{request.File.FileName}' for survey {survey.ReferenceCode}",
            entityType: "Evidence",
            entityId: evidence.Id,
            entityIdentifier: request.File.FileName,
            oldValues: null,
            newValues: System.Text.Json.JsonSerializer.Serialize(new
            {
                evidence.EvidenceType,
                evidence.OriginalFileName,
                evidence.FileSizeBytes,
                evidence.MimeType,
                request.SurveyId,
                SurveyReferenceCode = survey.ReferenceCode,
                request.PropertyUnitId,
                request.PersonPropertyRelationId
            }),
            changedFields: "New Property Photo",
            cancellationToken: cancellationToken
        );

        // Map to DTO
        var result = _mapper.Map<EvidenceDto>(evidence);
        result.IsExpired = evidence.IsExpired();

        return result;
    }
}