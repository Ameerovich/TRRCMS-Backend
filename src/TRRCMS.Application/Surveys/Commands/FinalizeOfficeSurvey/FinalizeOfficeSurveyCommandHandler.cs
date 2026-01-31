using AutoMapper;
using MediatR;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Common.Services;
using TRRCMS.Application.Surveys.Dtos;
using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Surveys.Commands.FinalizeOfficeSurvey;

/// <summary>
/// Handler for FinalizeOfficeSurveyCommand
/// </summary>
public class FinalizeOfficeSurveyCommandHandler : IRequestHandler<FinalizeOfficeSurveyCommand, OfficeSurveyFinalizationResultDto>
{
    private readonly ISurveyRepository _surveyRepository;
    private readonly IPropertyUnitRepository _propertyUnitRepository;
    private readonly IPersonPropertyRelationRepository _personPropertyRelationRepository;
    private readonly IHouseholdRepository _householdRepository;
    private readonly IPersonRepository _personRepository;
    private readonly IEvidenceRepository _evidenceRepository;
    private readonly IClaimRepository _claimRepository;
    private readonly IClaimNumberGenerator _claimNumberGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;
    private readonly IMapper _mapper;

    public FinalizeOfficeSurveyCommandHandler(
        ISurveyRepository surveyRepository,
        IPropertyUnitRepository propertyUnitRepository,
        IPersonPropertyRelationRepository personPropertyRelationRepository,
        IHouseholdRepository householdRepository,
        IPersonRepository personRepository,
        IEvidenceRepository evidenceRepository,
        IClaimRepository claimRepository,
        IClaimNumberGenerator claimNumberGenerator,
        ICurrentUserService currentUserService,
        IAuditService auditService,
        IMapper mapper)
    {
        _surveyRepository = surveyRepository ?? throw new ArgumentNullException(nameof(surveyRepository));
        _propertyUnitRepository = propertyUnitRepository ?? throw new ArgumentNullException(nameof(propertyUnitRepository));
        _personPropertyRelationRepository = personPropertyRelationRepository ?? throw new ArgumentNullException(nameof(personPropertyRelationRepository));
        _householdRepository = householdRepository ?? throw new ArgumentNullException(nameof(householdRepository));
        _personRepository = personRepository ?? throw new ArgumentNullException(nameof(personRepository));
        _evidenceRepository = evidenceRepository ?? throw new ArgumentNullException(nameof(evidenceRepository));
        _claimRepository = claimRepository ?? throw new ArgumentNullException(nameof(claimRepository));
        _claimNumberGenerator = claimNumberGenerator ?? throw new ArgumentNullException(nameof(claimNumberGenerator));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<OfficeSurveyFinalizationResultDto> Handle(
        FinalizeOfficeSurveyCommand request,
        CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated");

        var survey = await _surveyRepository.GetByIdAsync(request.SurveyId, cancellationToken)
            ?? throw new NotFoundException($"Survey with ID {request.SurveyId} not found");

        if (survey.Type != SurveyType.Office)
            throw new ValidationException("This endpoint is only for office surveys.");

        if (survey.Status != SurveyStatus.Draft)
            throw new ValidationException($"Survey is in {survey.Status} status. Only Draft surveys can be finalized.");

        var warnings = new List<string>();

        if (!survey.PropertyUnitId.HasValue)
            throw new ValidationException("Survey must have a property unit linked before finalization.");

        var propertyUnit = await _propertyUnitRepository.GetByIdAsync(survey.PropertyUnitId.Value, cancellationToken);
        var households = (await _householdRepository.GetByPropertyUnitIdAsync(survey.PropertyUnitId.Value, cancellationToken)).ToList();
        var relations = (await _personPropertyRelationRepository.GetByPropertyUnitIdAsync(survey.PropertyUnitId.Value, cancellationToken)).ToList();

        // Get evidence using EvidenceType? enum (null = no filter)
        var evidence = await _evidenceRepository.GetBySurveyContextAsync(survey.BuildingId, null, cancellationToken);

        var personCount = 0;
        foreach (var household in households)
        {
            var persons = await _personRepository.GetByHouseholdIdAsync(household.Id, cancellationToken);
            personCount += persons.Count;
        }

        // Count ownership relations using enum comparison
        var ownershipRelations = relations.Where(r =>
            r.RelationType == RelationType.Owner ||
            r.RelationType == RelationType.Heir).ToList();

        if (!households.Any()) warnings.Add("No households captured in this survey.");
        if (personCount == 0) warnings.Add("No persons captured in this survey.");
        if (!relations.Any()) warnings.Add("No person-property relations captured.");
        if (!evidence.Any()) warnings.Add("No evidence uploaded for this survey.");

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

        survey.MarkAsFinalized(currentUserId);

        Claim? createdClaim = null;
        string? claimNotCreatedReason = null;

        if (request.AutoCreateClaim)
        {
            if (ownershipRelations.Any())
            {
                var primaryOwnerRelation = ownershipRelations.First();
                var primaryOwner = await _personRepository.GetByIdAsync(primaryOwnerRelation.PersonId, cancellationToken);

                if (primaryOwner != null)
                {
                    var claimNumber = await _claimNumberGenerator.GenerateNextClaimNumberAsync(cancellationToken);
                    var hasEvidence = evidence.Any();
                    var lifecycleStage = hasEvidence ? LifecycleStage.DraftPendingSubmission : LifecycleStage.AwaitingDocuments;

                    createdClaim = Claim.Create(
                        claimNumber: claimNumber,
                        propertyUnitId: survey.PropertyUnitId.Value,
                        primaryClaimantId: primaryOwner.Id,
                        claimType: "Ownership Claim",
                        claimSource: ClaimSource.OfficeSubmission,
                        createdByUserId: currentUserId);

                    createdClaim.MoveToStage(lifecycleStage, currentUserId);
                    await _claimRepository.AddAsync(createdClaim, cancellationToken);
                    survey.LinkClaim(createdClaim.Id, currentUserId);

                    await _auditService.LogActionAsync(
                        actionType: AuditActionType.Create,
                        actionDescription: $"Created claim {claimNumber} from office survey {survey.ReferenceCode}",
                        entityType: "Claim",
                        entityId: createdClaim.Id,
                        entityIdentifier: claimNumber,
                        oldValues: null,
                        newValues: System.Text.Json.JsonSerializer.Serialize(new
                        {
                            createdClaim.ClaimNumber,
                            createdClaim.PropertyUnitId,
                            createdClaim.PrimaryClaimantId,
                            PrimaryClaimantName = $"{primaryOwner.FirstNameArabic} {primaryOwner.FamilyNameArabic}",
                            createdClaim.ClaimType,
                            createdClaim.ClaimSource,
                            createdClaim.LifecycleStage,
                            SourceSurveyId = survey.Id,
                            SourceSurveyReference = survey.ReferenceCode
                        }),
                        changedFields: "New Claim from Office Survey",
                        cancellationToken: cancellationToken);
                }
                else
                {
                    claimNotCreatedReason = "Primary owner person record not found.";
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

        await _surveyRepository.UpdateAsync(survey, cancellationToken);
        await _surveyRepository.SaveChangesAsync(cancellationToken);

        await _auditService.LogActionAsync(
            actionType: AuditActionType.StatusChange,
            actionDescription: $"Finalized office survey {survey.ReferenceCode}",
            entityType: "Survey",
            entityId: survey.Id,
            entityIdentifier: survey.ReferenceCode,
            oldValues: System.Text.Json.JsonSerializer.Serialize(new { Status = "Draft" }),
            newValues: System.Text.Json.JsonSerializer.Serialize(new
            {
                Status = "Finalized",
                FinalizedAt = DateTime.UtcNow,
                FinalizedBy = currentUserId,
                ClaimCreated = createdClaim != null,
                ClaimNumber = createdClaim?.ClaimNumber,
                DataSummary = new
                {
                    PropertyUnits = 1,
                    Households = households.Count,
                    Persons = personCount,
                    Relations = relations.Count,
                    OwnershipRelations = ownershipRelations.Count,
                    Evidence = evidence.Count
                }
            }),
            changedFields: "Status, FinalizedAt, FinalizedBy",
            cancellationToken: cancellationToken);

        var totalEvidenceSize = evidence.Sum(e => e.FileSizeBytes);

        var result = new OfficeSurveyFinalizationResultDto
        {
            Survey = _mapper.Map<SurveyDto>(survey),
            ClaimCreated = createdClaim != null,
            ClaimId = createdClaim?.Id,
            ClaimNumber = createdClaim?.ClaimNumber,
            ClaimNotCreatedReason = claimNotCreatedReason,
            Warnings = warnings,
            DataSummary = new SurveyDataSummaryDto
            {
                PropertyUnitsCount = 1,
                HouseholdsCount = households.Count,
                PersonsCount = personCount,
                RelationsCount = relations.Count,
                OwnershipRelationsCount = ownershipRelations.Count,
                EvidenceCount = evidence.Count,
                TotalEvidenceSizeBytes = totalEvidenceSize
            }
        };

        result.Survey.FieldCollectorName = _currentUserService.Username;

        return result;
    }
}
