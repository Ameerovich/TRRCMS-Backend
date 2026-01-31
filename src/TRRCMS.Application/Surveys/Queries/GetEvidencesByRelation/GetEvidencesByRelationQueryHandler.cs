using MediatR;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Evidences.Dtos;
using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Surveys.Queries.GetEvidencesByRelation;

public class GetEvidencesByRelationQueryHandler : IRequestHandler<GetEvidencesByRelationQuery, List<EvidenceDto>>
{
    private readonly ISurveyRepository _surveyRepository;
    private readonly IPersonPropertyRelationRepository _relationRepository;
    private readonly IPropertyUnitRepository _propertyUnitRepository;
    private readonly IEvidenceRepository _evidenceRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetEvidencesByRelationQueryHandler(
        ISurveyRepository surveyRepository,
        IPersonPropertyRelationRepository relationRepository,
        IPropertyUnitRepository propertyUnitRepository,
        IEvidenceRepository evidenceRepository,
        ICurrentUserService currentUserService)
    {
        _surveyRepository = surveyRepository;
        _relationRepository = relationRepository;
        _propertyUnitRepository = propertyUnitRepository;
        _evidenceRepository = evidenceRepository;
        _currentUserService = currentUserService;
    }

    public async Task<List<EvidenceDto>> Handle(GetEvidencesByRelationQuery request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated");

        // Get and validate survey
        var survey = await _surveyRepository.GetByIdAsync(request.SurveyId, cancellationToken)
            ?? throw new NotFoundException($"Survey with ID {request.SurveyId} not found");

        if (survey.FieldCollectorId != currentUserId)
            throw new UnauthorizedAccessException("You can only view evidences for your own surveys");

        // Get and validate relation
        var relation = await _relationRepository.GetByIdAsync(request.RelationId, cancellationToken)
            ?? throw new NotFoundException($"Relation with ID {request.RelationId} not found");

        // Verify relation belongs to survey's building
        var propertyUnit = await _propertyUnitRepository.GetByIdAsync(relation.PropertyUnitId, cancellationToken)
            ?? throw new NotFoundException($"Property unit not found");

        if (propertyUnit.BuildingId != survey.BuildingId)
            throw new ValidationException("Relation does not belong to this survey's building");

        // Get evidences for this relation
        var evidences = await _evidenceRepository.GetByRelationIdAsync(
            request.RelationId,
            request.EvidenceType,
            request.OnlyCurrentVersions,
            cancellationToken);

        return evidences.Select(MapToDto).ToList();
    }

    private static EvidenceDto MapToDto(Evidence e)
    {
        return new EvidenceDto
        {
            Id = e.Id,
            EvidenceType = e.EvidenceType,
            Description = e.Description,
            OriginalFileName = e.OriginalFileName,
            FilePath = e.FilePath,
            FileSizeBytes = e.FileSizeBytes,
            MimeType = e.MimeType,
            FileHash = e.FileHash,
            DocumentIssuedDate = e.DocumentIssuedDate,
            DocumentExpiryDate = e.DocumentExpiryDate,
            IssuingAuthority = e.IssuingAuthority,
            DocumentReferenceNumber = e.DocumentReferenceNumber,
            Notes = e.Notes,
            VersionNumber = e.VersionNumber,
            PreviousVersionId = e.PreviousVersionId,
            IsCurrentVersion = e.IsCurrentVersion,
            PersonId = e.PersonId,
            PersonPropertyRelationId = e.PersonPropertyRelationId,
            ClaimId = e.ClaimId,
            CreatedAtUtc = e.CreatedAtUtc,
            CreatedBy = e.CreatedBy,
            LastModifiedAtUtc = e.LastModifiedAtUtc,
            LastModifiedBy = e.LastModifiedBy,
            IsDeleted = e.IsDeleted,
            DeletedAtUtc = e.DeletedAtUtc,
            DeletedBy = e.DeletedBy,
            IsExpired = e.IsExpired()
        };
    }
}
