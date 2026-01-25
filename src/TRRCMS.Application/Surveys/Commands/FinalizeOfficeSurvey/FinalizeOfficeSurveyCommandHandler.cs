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
/// Finalizes office survey and optionally creates a claim
/// UC-004 S21: Mark as finalized, UC-005: Complete draft office survey
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
        // Get current user
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated");

        // Get existing survey with building info
        var survey = await _surveyRepository.GetByIdAsync(request.SurveyId, cancellationToken);
        if (survey == null)
        {
            throw new NotFoundException($"Survey with ID {request.SurveyId} not found");
        }

        // Verify this is an office survey
        if (survey.Type != SurveyType.Office)
        {
            throw new ValidationException("This endpoint is only for office surveys.");
        }

        // Verify survey is in Draft status
        if (survey.Status != SurveyStatus.Draft)
        {
            throw new ValidationException($"Survey is in {survey.Status} status. Only Draft surveys can be finalized.");
        }

        // Collect warnings
        var warnings = new List<string>();

        // Validate survey has minimum required data
        if (!survey.PropertyUnitId.HasValue)
        {
            throw new ValidationException("Survey must have a property unit linked before finalization.");
        }

        // Get related data for summary
        var propertyUnit = await _propertyUnitRepository.GetByIdAsync(survey.PropertyUnitId.Value, cancellationToken);

        // Use correct method names from repositories
        var households = (await _householdRepository.GetByPropertyUnitIdAsync(survey.PropertyUnitId.Value, cancellationToken)).ToList();
        var relations = (await _personPropertyRelationRepository.GetByPropertyUnitIdAsync(survey.PropertyUnitId.Value, cancellationToken)).ToList();

        // Get evidence by survey context (building-based)
        var evidence = await _evidenceRepository.GetBySurveyContextAsync(survey.BuildingId, null, cancellationToken);

        // Count persons from households using correct method name
        var personCount = 0;
        foreach (var household in households)
        {
            var persons = await _personRepository.GetByHouseholdIdAsync(household.Id, cancellationToken);
            personCount += persons.Count;
        }

        // Count ownership relations (basis for claim creation)
        // RelationType in entity is string, so compare with string values
        var ownershipRelations = relations.Where(r =>
            r.RelationType.Equals("Owner", StringComparison.OrdinalIgnoreCase) ||
            r.RelationType.Equals("Heir", StringComparison.OrdinalIgnoreCase) ||
            r.RelationType.Equals("Heirs", StringComparison.OrdinalIgnoreCase)).ToList();

        // Add warnings for missing data
        if (!households.Any())
        {
            warnings.Add("No households captured in this survey.");
        }
        if (personCount == 0)
        {
            warnings.Add("No persons captured in this survey.");
        }
        if (!relations.Any())
        {
            warnings.Add("No person-property relations captured.");
        }
        if (!evidence.Any())
        {
            warnings.Add("No evidence uploaded for this survey.");
        }

        // Update survey with final notes if provided
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
                modifiedByUserId: currentUserId
            );
        }

        // Mark survey as finalized
        survey.MarkAsFinalized(currentUserId);

        // Create claim if ownership relations exist and auto-create is enabled
        Claim? createdClaim = null;
        string? claimNotCreatedReason = null;

        if (request.AutoCreateClaim)
        {
            if (ownershipRelations.Any())
            {
                // Get primary owner (first ownership relation)
                var primaryOwnerRelation = ownershipRelations.First();
                var primaryOwner = await _personRepository.GetByIdAsync(primaryOwnerRelation.PersonId, cancellationToken);

                if (primaryOwner != null)
                {
                    // Generate claim number using correct method name
                    var claimNumber = await _claimNumberGenerator.GenerateNextClaimNumberAsync(cancellationToken);

                    // Determine initial lifecycle stage based on evidence
                    var hasEvidence = evidence.Any();
                    var lifecycleStage = hasEvidence
                        ? LifecycleStage.DraftPendingSubmission
                        : LifecycleStage.AwaitingDocuments;

                    // Create claim using correct ClaimSource enum value
                    // Note: primaryClaimantId is Guid? in Create method signature
                    createdClaim = Claim.Create(
                        claimNumber: claimNumber,
                        propertyUnitId: survey.PropertyUnitId.Value,
                        primaryClaimantId: primaryOwner.Id, // Guid is implicitly convertible to Guid?
                        claimType: "Ownership Claim", // Default type
                        claimSource: ClaimSource.OfficeSubmission, // Correct enum value
                        createdByUserId: currentUserId
                    );

                    // Set initial lifecycle stage using correct method name
                    createdClaim.MoveToStage(lifecycleStage, currentUserId);

                    // Save claim
                    await _claimRepository.AddAsync(createdClaim, cancellationToken);

                    // Link claim to survey
                    survey.LinkClaim(createdClaim.Id, currentUserId);

                    // Audit log for claim creation
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
                        cancellationToken: cancellationToken
                    );
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

        // Save all changes
        await _surveyRepository.UpdateAsync(survey, cancellationToken);
        await _surveyRepository.SaveChangesAsync(cancellationToken);

        // Audit log for finalization
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
            cancellationToken: cancellationToken
        );

        // Calculate total evidence size
        var totalEvidenceSize = evidence.Sum(e => e.FileSizeBytes);

        // Build result
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
                PropertyUnitsCount = 1, // Survey is linked to one property unit
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