using AutoMapper;
using MediatR;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Evidences.Dtos;
using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Surveys.Commands.UploadTenureDocument;

/// <summary>
/// Handler for UploadTenureDocumentCommand
/// Uploads tenure document and links to person-property relation
/// </summary>
public class UploadTenureDocumentCommandHandler : IRequestHandler<UploadTenureDocumentCommand, EvidenceDto>
{
    private readonly ISurveyRepository _surveyRepository;
    private readonly IPersonPropertyRelationRepository _relationRepository;
    private readonly IPropertyUnitRepository _propertyUnitRepository;
    private readonly IEvidenceRepository _evidenceRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;
    private readonly IMapper _mapper;

    public UploadTenureDocumentCommandHandler(
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

    public async Task<EvidenceDto> Handle(UploadTenureDocumentCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated");

        var survey = await _surveyRepository.GetByIdAsync(request.SurveyId, cancellationToken)
            ?? throw new NotFoundException($"Survey with ID {request.SurveyId} not found");

        if (survey.FieldCollectorId != currentUserId)
            throw new UnauthorizedAccessException("You can only upload evidence for your own surveys");

        if (survey.Status != SurveyStatus.Draft)
            throw new ValidationException($"Cannot upload evidence for survey in {survey.Status} status. Only Draft surveys can be modified.");

        var relation = await _relationRepository.GetByIdAsync(request.PersonPropertyRelationId, cancellationToken)
            ?? throw new NotFoundException($"Person-property relation with ID {request.PersonPropertyRelationId} not found");

        var propertyUnit = await _propertyUnitRepository.GetByIdAsync(relation.PropertyUnitId, cancellationToken)
            ?? throw new NotFoundException($"Property unit with ID {relation.PropertyUnitId} not found");

        if (propertyUnit.BuildingId != survey.BuildingId)
            throw new ValidationException("Person-property relation does not belong to the survey's building");

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
                stream, request.File.FileName, "tenure-documents", request.SurveyId, cancellationToken);
        }

        var mimeType = _fileStorageService.GetMimeType(request.File.FileName);

        // Create Evidence entity using EvidenceType enum
        var evidence = Evidence.Create(
            evidenceType: request.EvidenceType,
            description: request.Description ?? request.File.FileName,
            originalFileName: request.File.FileName,
            filePath: filePath,
            fileSizeBytes: request.File.Length,
            mimeType: mimeType,
            fileHash: fileHash,
            createdByUserId: currentUserId);

        evidence.LinkToRelation(request.PersonPropertyRelationId, currentUserId);

        evidence.UpdateMetadata(
            issuedDate: request.DocumentIssuedDate,
            expiryDate: request.DocumentExpiryDate,
            issuingAuthority: request.IssuingAuthority,
            referenceNumber: request.DocumentReferenceNumber,
            notes: request.Notes,
            modifiedByUserId: currentUserId);

        await _evidenceRepository.AddAsync(evidence, cancellationToken);
        await _evidenceRepository.SaveChangesAsync(cancellationToken);

        await _auditService.LogActionAsync(
            actionType: AuditActionType.Create,
            actionDescription: $"Uploaded tenure document '{request.File.FileName}' ({request.EvidenceType}) for person-property relation in survey {survey.ReferenceCode}",
            entityType: "Evidence",
            entityId: evidence.Id,
            entityIdentifier: request.File.FileName,
            oldValues: null,
            newValues: System.Text.Json.JsonSerializer.Serialize(new
            {
                EvidenceType = evidence.EvidenceType.ToString(),
                evidence.OriginalFileName,
                evidence.FileSizeBytes,
                request.PersonPropertyRelationId,
                RelationType = relation.RelationType.ToString(),
                PropertyUnitId = relation.PropertyUnitId,
                request.SurveyId,
                SurveyReferenceCode = survey.ReferenceCode
            }),
            changedFields: "New Tenure Document",
            cancellationToken: cancellationToken);

        var result = _mapper.Map<EvidenceDto>(evidence);
        result.IsExpired = evidence.IsExpired();

        return result;
    }
}
