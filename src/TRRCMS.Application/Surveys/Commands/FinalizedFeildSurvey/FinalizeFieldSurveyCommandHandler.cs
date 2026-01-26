using AutoMapper;
using MediatR;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Surveys.Dtos;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Surveys.Commands.FinalizeFieldSurvey;

/// <summary>
/// Handler for FinalizeFieldSurveyCommand
/// Finalizes field survey for export to .uhc container
/// UC-001 Final Stage: Complete field survey
/// Note: Unlike office surveys, field surveys don't auto-create claims.
/// Claims are created during import (UC-003) after validation.
/// </summary>
public class FinalizeFieldSurveyCommandHandler : IRequestHandler<FinalizeFieldSurveyCommand, FieldSurveyFinalizationResultDto>
{
    private readonly ISurveyRepository _surveyRepository;
    private readonly IPropertyUnitRepository _propertyUnitRepository;
    private readonly IPersonPropertyRelationRepository _personPropertyRelationRepository;
    private readonly IHouseholdRepository _householdRepository;
    private readonly IPersonRepository _personRepository;
    private readonly IEvidenceRepository _evidenceRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;
    private readonly IMapper _mapper;

    public FinalizeFieldSurveyCommandHandler(
        ISurveyRepository surveyRepository,
        IPropertyUnitRepository propertyUnitRepository,
        IPersonPropertyRelationRepository personPropertyRelationRepository,
        IHouseholdRepository householdRepository,
        IPersonRepository personRepository,
        IEvidenceRepository evidenceRepository,
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
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<FieldSurveyFinalizationResultDto> Handle(
        FinalizeFieldSurveyCommand request,
        CancellationToken cancellationToken)
    {
        // Get current user (field collector)
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated");

        // Get existing survey with building info
        var survey = await _surveyRepository.GetByIdAsync(request.SurveyId, cancellationToken);
        if (survey == null)
        {
            throw new NotFoundException($"Survey with ID {request.SurveyId} not found");
        }

        // Verify this is a field survey
        if (survey.Type != SurveyType.Field)
        {
            throw new ValidationException("This endpoint is only for field surveys. Use /api/surveys/office/{id}/finalize for office surveys.");
        }

        // Verify survey is in Draft status
        if (survey.Status != SurveyStatus.Draft)
        {
            throw new ValidationException($"Survey is in {survey.Status} status. Only Draft surveys can be finalized.");
        }

        // Verify current user is the field collector or has supervisor permissions
        if (survey.FieldCollectorId != currentUserId)
        {
            // TODO: Add supervisor permission check
            throw new UnauthorizedAccessException("You can only finalize your own field surveys.");
        }

        // Collect warnings for incomplete data
        var warnings = new List<string>();

        // Validate survey has minimum required data
        if (!survey.PropertyUnitId.HasValue)
        {
            throw new ValidationException("Survey must have a property unit linked before finalization.");
        }

        // Get related data for summary
        var propertyUnit = await _propertyUnitRepository.GetByIdAsync(survey.PropertyUnitId.Value, cancellationToken);

        var households = (await _householdRepository.GetByPropertyUnitIdAsync(
            survey.PropertyUnitId.Value, cancellationToken)).ToList();
        var relations = (await _personPropertyRelationRepository.GetByPropertyUnitIdAsync(
            survey.PropertyUnitId.Value, cancellationToken)).ToList();

        // Get evidence by survey context (building-based)
        // CORRECT signature: GetBySurveyContextAsync(buildingId, evidenceType?, cancellationToken)
        var evidence = await _evidenceRepository.GetBySurveyContextAsync(
            survey.BuildingId,
            null, // No filter on evidence type
            cancellationToken);

        // Count persons from households
        var personCount = 0;
        foreach (var household in households)
        {
            var persons = await _personRepository.GetByHouseholdIdAsync(household.Id, cancellationToken);
            personCount += persons.Count;
        }

        // Count ownership relations
        var ownershipRelations = relations.Where(r =>
            r.RelationType.Equals("Owner", StringComparison.OrdinalIgnoreCase) ||
            r.RelationType.Equals("Heir", StringComparison.OrdinalIgnoreCase) ||
            r.RelationType.Equals("Heirs", StringComparison.OrdinalIgnoreCase)).ToList();

        // Add completeness warnings if validation is enabled
        if (request.ValidateCompleteness)
        {
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
            if (!ownershipRelations.Any())
            {
                warnings.Add("No ownership relations found. A claim cannot be created without ownership evidence.");
            }
            if (string.IsNullOrWhiteSpace(survey.GpsCoordinates) && string.IsNullOrWhiteSpace(request.FinalGpsCoordinates))
            {
                warnings.Add("GPS coordinates not recorded.");
            }
            if (string.IsNullOrWhiteSpace(survey.IntervieweeName))
            {
                warnings.Add("Interviewee name not recorded.");
            }
        }

        // Update survey with final data if provided
        var finalGps = request.FinalGpsCoordinates ?? survey.GpsCoordinates;
        var finalNotes = survey.Notes;

        if (!string.IsNullOrWhiteSpace(request.FinalNotes))
        {
            finalNotes = string.IsNullOrWhiteSpace(finalNotes)
                ? $"[Final Notes]: {request.FinalNotes}"
                : $"{finalNotes}\n[Final Notes]: {request.FinalNotes}";
        }

        survey.UpdateSurveyDetails(
            gpsCoordinates: finalGps,
            intervieweeName: survey.IntervieweeName,
            intervieweeRelationship: survey.IntervieweeRelationship,
            notes: finalNotes,
            durationMinutes: request.DurationMinutes ?? survey.DurationMinutes,
            modifiedByUserId: currentUserId
        );

        // Mark survey as finalized
        survey.MarkAsFinalized(currentUserId);

        // Save changes
        await _surveyRepository.UpdateAsync(survey, cancellationToken);
        await _surveyRepository.SaveChangesAsync(cancellationToken);

        // Audit log for finalization
        await _auditService.LogActionAsync(
            actionType: AuditActionType.StatusChange,
            actionDescription: $"Finalized field survey {survey.ReferenceCode}",
            entityType: "Survey",
            entityId: survey.Id,
            entityIdentifier: survey.ReferenceCode,
            oldValues: System.Text.Json.JsonSerializer.Serialize(new { Status = "Draft" }),
            newValues: System.Text.Json.JsonSerializer.Serialize(new
            {
                Status = "Finalized",
                FinalizedAt = DateTime.UtcNow,
                FinalizedBy = currentUserId,
                ReadyForExport = true,
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
        var result = new FieldSurveyFinalizationResultDto
        {
            Survey = _mapper.Map<SurveyDto>(survey),
            IsReadyForExport = true,
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

        // Add building info to result
        if (survey.Building != null)
        {
            result.Survey.BuildingNumber = survey.Building.BuildingNumber;
            result.Survey.BuildingAddress = survey.Building.Address;
        }

        result.Survey.FieldCollectorName = _currentUserService.Username;

        return result;
    }
}