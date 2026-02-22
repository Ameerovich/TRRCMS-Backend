using AutoMapper;
using MediatR;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Common.Services;
using TRRCMS.Application.Surveys.Dtos;
using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Surveys.Commands.ProcessOfficeSurveyClaims;

/// <summary>
/// Handler for ProcessOfficeSurveyClaimsCommand.
/// Creates one claim per ownership/heir relation found in the survey.
/// </summary>
public class ProcessOfficeSurveyClaimsCommandHandler : IRequestHandler<ProcessOfficeSurveyClaimsCommand, OfficeSurveyFinalizationResultDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClaimNumberGenerator _claimNumberGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;
    private readonly IMapper _mapper;

    public ProcessOfficeSurveyClaimsCommandHandler(
        IUnitOfWork unitOfWork,
        IClaimNumberGenerator claimNumberGenerator,
        ICurrentUserService currentUserService,
        IAuditService auditService,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _claimNumberGenerator = claimNumberGenerator ?? throw new ArgumentNullException(nameof(claimNumberGenerator));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<OfficeSurveyFinalizationResultDto> Handle(
        ProcessOfficeSurveyClaimsCommand request,
        CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated");

        var survey = await _unitOfWork.Surveys.GetByIdAsync(request.SurveyId, cancellationToken)
            ?? throw new NotFoundException($"Survey with ID {request.SurveyId} not found");

        if (survey.Type != Domain.Enums.SurveyType.Office)
            throw new ValidationException("This endpoint is only for office surveys.");

        if (survey.Status != SurveyStatus.Draft)
            throw new ValidationException($"Survey is in {survey.Status} status. Only Draft surveys can be processed.");

        var warnings = new List<string>();

        if (!survey.PropertyUnitId.HasValue)
            throw new ValidationException("Survey must have a property unit linked before processing.");

        var propertyUnit = await _unitOfWork.PropertyUnits.GetByIdAsync(survey.PropertyUnitId.Value, cancellationToken);
        var households = (await _unitOfWork.Households.GetByPropertyUnitIdAsync(survey.PropertyUnitId.Value, cancellationToken)).ToList();

        // ── CRITICAL: Only fetch relations created within THIS survey ──
        // This ensures claims are scoped to the current survey only,
        // not to all relations ever created for this property unit.
        var surveyRelations = (await _unitOfWork.PersonPropertyRelations.GetBySurveyIdWithEvidencesAsync(
            request.SurveyId, cancellationToken)).ToList();

        // Also get all relations on the property unit for the data summary (informational)
        var allPropertyUnitRelations = (await _unitOfWork.PersonPropertyRelations.GetByPropertyUnitIdWithEvidencesAsync(
            survey.PropertyUnitId.Value, cancellationToken)).ToList();

        // Get all evidence for the survey context (for the data summary)
        var allEvidence = await _unitOfWork.Evidences.GetBySurveyContextAsync(survey.BuildingId, null, cancellationToken);

        var personCount = 0;
        foreach (var household in households)
        {
            var persons = await _unitOfWork.Persons.GetByHouseholdIdAsync(household.Id, cancellationToken);
            personCount += persons.Count;
        }

        // Filter ownership/heir relations — ONLY from this survey's relations
        var ownershipRelations = surveyRelations.Where(r =>
            r.RelationType == RelationType.Owner ||
            r.RelationType == RelationType.Heir).ToList();

        if (!households.Any()) warnings.Add("No households captured in this survey.");
        if (personCount == 0) warnings.Add("No persons captured in this survey.");
        if (!surveyRelations.Any()) warnings.Add("No person-property relations created in this survey.");
        if (!allEvidence.Any()) warnings.Add("No evidence uploaded for this survey.");

        // ── Apply final notes / duration ──
        if (!string.IsNullOrWhiteSpace(request.FinalNotes) || request.DurationMinutes.HasValue)
        {
            var finalNotes = survey.Notes;
            if (!string.IsNullOrWhiteSpace(request.FinalNotes))
            {
                finalNotes = string.IsNullOrWhiteSpace(finalNotes)
                    ? $"[Final Notes]: {request.FinalNotes}"
                    : $"{finalNotes}\n[Final Notes]: {request.FinalNotes}";
            }

            survey.UpdateSurveyDetails(
                gpsCoordinates: null,
                intervieweeName: survey.IntervieweeName,
                intervieweeRelationship: survey.IntervieweeRelationship,
                notes: finalNotes,
                durationMinutes: request.DurationMinutes ?? survey.DurationMinutes,
                modifiedByUserId: currentUserId);
        }

        // ── Create claims — one per qualifying relation ──
        var createdClaims = new List<CreatedClaimSummaryDto>();
        string? claimNotCreatedReason = null;

        if (request.AutoCreateClaim)
        {
            if (ownershipRelations.Any())
            {
                // Derive TypeOfWorks once from the property unit
                var typeOfWorks = MapPropertyUnitTypeToTypeOfWorks(propertyUnit?.UnitType);

                foreach (var relation in ownershipRelations)
                {
                    var person = await _unitOfWork.Persons.GetByIdAsync(relation.PersonId, cancellationToken);
                    if (person == null)
                    {
                        warnings.Add($"Person record not found for relation {relation.Id}. Claim skipped.");
                        continue;
                    }

                    var claimNumber = await _claimNumberGenerator.GenerateNextClaimNumberAsync(cancellationToken);

                    // Check if this specific relation has tenure evidence attached
                    var relationHasEvidence = relation.EvidenceRelations != null && relation.EvidenceRelations.Any(er => er.IsActive && !er.IsDeleted);

                    var lifecycleStage = relationHasEvidence
                        ? LifecycleStage.DraftPendingSubmission
                        : LifecycleStage.AwaitingDocuments;

                    var claim = Claim.Create(
                        claimNumber: claimNumber,
                        propertyUnitId: survey.PropertyUnitId.Value,
                        primaryClaimantId: person.Id,
                        claimType: "Ownership Claim",
                        claimSource: ClaimSource.OfficeSubmission,
                        createdByUserId: currentUserId);

                    claim.MoveToStage(lifecycleStage, currentUserId);
                    await _unitOfWork.Claims.AddAsync(claim, cancellationToken);

                    // Link first claim to survey for backward compatibility
                    if (createdClaims.Count == 0)
                    {
                        survey.LinkClaim(claim.Id, currentUserId);
                    }

                    // Build the UI summary DTO
                    createdClaims.Add(new CreatedClaimSummaryDto
                    {
                        ClaimId = claim.Id,
                        ClaimNumber = claimNumber,
                        PropertyUnitIdNumber = propertyUnit?.UnitIdentifier ?? string.Empty,
                        FullNameArabic = person.GetFullNameArabic(),
                        ClaimSource = (int)ClaimSource.OfficeSubmission,
                        CasePriority = (int)CasePriority.Normal,
                        ClaimStatus = (int)ClaimStatus.Draft,
                        SurveyDate = survey.CreatedAtUtc,
                        TypeOfWorks = typeOfWorks,
                        HasEvidence = relationHasEvidence,
                        SourceRelationId = relation.Id,
                        RelationType = (int)relation.RelationType,
                        PersonId = person.Id,
                        PropertyUnitId = survey.PropertyUnitId.Value
                    });

                    // Audit each claim creation
                    await _auditService.LogActionAsync(
                        actionType: AuditActionType.Create,
                        actionDescription: $"Created claim {claimNumber} from office survey {survey.ReferenceCode} (relation {relation.Id})",
                        entityType: "Claim",
                        entityId: claim.Id,
                        entityIdentifier: claimNumber,
                        oldValues: null,
                        newValues: System.Text.Json.JsonSerializer.Serialize(new
                        {
                            claim.ClaimNumber,
                            claim.PropertyUnitId,
                            claim.PrimaryClaimantId,
                            PrimaryClaimantName = person.GetFullNameArabic(),
                            claim.ClaimType,
                            claim.ClaimSource,
                            claim.LifecycleStage,
                            RelationId = relation.Id,
                            RelationType = relation.RelationType.ToString(),
                            SourceSurveyId = survey.Id,
                            SourceSurveyReference = survey.ReferenceCode
                        }),
                        changedFields: "New Claim from Office Survey",
                        cancellationToken: cancellationToken);
                }

                if (!createdClaims.Any())
                {
                    claimNotCreatedReason = "Ownership relations found but all associated person records were missing.";
                    warnings.Add(claimNotCreatedReason);
                }
            }
            else
            {
                claimNotCreatedReason = "No ownership relations found. Claim creation requires at least one owner or heir relation.";
            }
        }
        else
        {
            claimNotCreatedReason = "Auto-create claim was disabled.";
        }

        await _unitOfWork.Surveys.UpdateAsync(survey, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ── Audit claim processing ──
        await _auditService.LogActionAsync(
            actionType: AuditActionType.Update,
            actionDescription: $"Processed claims for office survey {survey.ReferenceCode}",
            entityType: "Survey",
            entityId: survey.Id,
            entityIdentifier: survey.ReferenceCode,
            oldValues: null,
            newValues: System.Text.Json.JsonSerializer.Serialize(new
            {
                ClaimsCreated = createdClaims.Count,
                ClaimNumbers = createdClaims.Select(c => c.ClaimNumber).ToList(),
                DataSummary = new
                {
                    PropertyUnits = 1,
                    Households = households.Count,
                    Persons = personCount,
                    SurveyRelations = surveyRelations.Count,
                    TotalPropertyUnitRelations = allPropertyUnitRelations.Count,
                    OwnershipRelations = ownershipRelations.Count,
                    Evidence = allEvidence.Count
                }
            }),
            changedFields: "Claims",
            cancellationToken: cancellationToken);

        // ── Build response (same shape as before) ──
        var totalEvidenceSize = allEvidence.Sum(e => e.FileSizeBytes);

        var result = new OfficeSurveyFinalizationResultDto
        {
            Survey = _mapper.Map<SurveyDto>(survey),
            ClaimCreated = createdClaims.Any(),
            ClaimId = createdClaims.FirstOrDefault()?.ClaimId,
            ClaimNumber = createdClaims.FirstOrDefault()?.ClaimNumber,
            ClaimsCreatedCount = createdClaims.Count,
            CreatedClaims = createdClaims,
            ClaimNotCreatedReason = claimNotCreatedReason,
            Warnings = warnings,
            DataSummary = new SurveyDataSummaryDto
            {
                PropertyUnitsCount = 1,
                HouseholdsCount = households.Count,
                PersonsCount = personCount,
                RelationsCount = surveyRelations.Count,
                OwnershipRelationsCount = ownershipRelations.Count,
                EvidenceCount = allEvidence.Count,
                TotalEvidenceSizeBytes = totalEvidenceSize
            }
        };

        result.Survey.FieldCollectorName = _currentUserService.Username;

        return result;
    }

    /// <summary>
    /// Maps PropertyUnitType enum to the TypeOfWorks string for the UI.
    /// Residential = سكني, Commercial = تجاري, Factorial = صناعي
    /// </summary>
    private static string MapPropertyUnitTypeToTypeOfWorks(PropertyUnitType? unitType)
    {
        return unitType switch
        {
            PropertyUnitType.Apartment => "Residential",
            PropertyUnitType.Shop => "Commercial",
            PropertyUnitType.Office => "Commercial",
            PropertyUnitType.Warehouse => "Factorial",
            _ => "Other"
        };
    }
}
