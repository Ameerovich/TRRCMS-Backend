using AutoMapper;
using MediatR;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Evidences.Dtos;
using TRRCMS.Application.Households.Dtos;
using TRRCMS.Application.PersonPropertyRelations.Dtos;
using TRRCMS.Application.Persons.Dtos;
using TRRCMS.Application.PropertyUnits.Dtos;
using TRRCMS.Application.Surveys.Dtos;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Surveys.Queries.GetFieldSurveyById;

/// <summary>
/// Handler for GetFieldSurveyByIdQuery
/// Returns field survey with all related data
/// UC-001/UC-002: Field Survey view/resume
/// </summary>
public class GetFieldSurveyByIdQueryHandler : IRequestHandler<GetFieldSurveyByIdQuery, FieldSurveyDetailDto>
{
    private readonly ISurveyRepository _surveyRepository;
    private readonly IPropertyUnitRepository _propertyUnitRepository;
    private readonly IHouseholdRepository _householdRepository;
    private readonly IPersonRepository _personRepository;
    private readonly IPersonPropertyRelationRepository _personPropertyRelationRepository;
    private readonly IEvidenceRepository _evidenceRepository;
    private readonly IUserRepository _userRepository;
    private readonly IClaimRepository _claimRepository;
    private readonly IMapper _mapper;

    public GetFieldSurveyByIdQueryHandler(
        ISurveyRepository surveyRepository,
        IPropertyUnitRepository propertyUnitRepository,
        IHouseholdRepository householdRepository,
        IPersonRepository personRepository,
        IPersonPropertyRelationRepository personPropertyRelationRepository,
        IEvidenceRepository evidenceRepository,
        IUserRepository userRepository,
        IClaimRepository claimRepository,
        IMapper mapper)
    {
        _surveyRepository = surveyRepository ?? throw new ArgumentNullException(nameof(surveyRepository));
        _propertyUnitRepository = propertyUnitRepository ?? throw new ArgumentNullException(nameof(propertyUnitRepository));
        _householdRepository = householdRepository ?? throw new ArgumentNullException(nameof(householdRepository));
        _personRepository = personRepository ?? throw new ArgumentNullException(nameof(personRepository));
        _personPropertyRelationRepository = personPropertyRelationRepository ?? throw new ArgumentNullException(nameof(personPropertyRelationRepository));
        _evidenceRepository = evidenceRepository ?? throw new ArgumentNullException(nameof(evidenceRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _claimRepository = claimRepository ?? throw new ArgumentNullException(nameof(claimRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<FieldSurveyDetailDto> Handle(
        GetFieldSurveyByIdQuery request,
        CancellationToken cancellationToken)
    {
        // Get survey with building and property unit
        var survey = await _surveyRepository.GetByIdAsync(request.SurveyId, cancellationToken);
        if (survey == null)
        {
            throw new NotFoundException($"Survey with ID {request.SurveyId} not found");
        }

        // Verify this is a field survey
        if (survey.Type != SurveyType.Field)
        {
            throw new ValidationException("This endpoint is only for field surveys. Use /api/surveys/office/{id} for office surveys.");
        }

        // Map base survey to DTO
        var result = new FieldSurveyDetailDto
        {
            Id = survey.Id,
            ReferenceCode = survey.ReferenceCode,
            BuildingId = survey.BuildingId,
            PropertyUnitId = survey.PropertyUnitId,
            FieldCollectorId = survey.FieldCollectorId,
            SurveyDate = survey.SurveyDate,
            Status = survey.Status.ToString(),
            SurveyType = survey.SurveyType,
            GpsCoordinates = survey.GpsCoordinates,
            IntervieweeName = survey.IntervieweeName,
            IntervieweeRelationship = survey.IntervieweeRelationship,
            Notes = survey.Notes,
            DurationMinutes = survey.DurationMinutes,
            ExportedDate = survey.ExportedDate,
            ExportPackageId = survey.ExportPackageId,
            ImportedDate = survey.ImportedDate,
            CreatedAtUtc = survey.CreatedAtUtc,
            CreatedBy = survey.CreatedBy,
            LastModifiedAtUtc = survey.LastModifiedAtUtc,
            LastModifiedBy = survey.LastModifiedBy
        };

        // Add building info
        if (survey.Building != null)
        {
            result.BuildingNumber = survey.Building.BuildingNumber;
            result.BuildingAddress = survey.Building.Address;
        }

        // Get field collector name
        var fieldCollector = await _userRepository.GetByIdAsync(survey.FieldCollectorId, cancellationToken);
        if (fieldCollector != null)
        {
            result.FieldCollectorName = fieldCollector.FullNameArabic ?? fieldCollector.Username;
        }

        // Get creator name
        var creator = await _userRepository.GetByIdAsync(survey.CreatedBy, cancellationToken);
        if (creator != null)
        {
            result.CreatedByName = creator.FullNameArabic ?? creator.Username;
        }

        // Initialize data summary
        result.DataSummary = new SurveyDataSummaryDto();

        // Get property unit details if linked
        if (survey.PropertyUnitId.HasValue)
        {
            var propertyUnit = await _propertyUnitRepository.GetByIdAsync(survey.PropertyUnitId.Value, cancellationToken);
            if (propertyUnit != null)
            {
                result.UnitIdentifier = propertyUnit.UnitIdentifier;
                result.PropertyUnit = _mapper.Map<PropertyUnitDto>(propertyUnit);
                result.DataSummary.PropertyUnitsCount = 1;

                // Get households for this property unit
                if (request.IncludeHouseholds)
                {
                    var households = await _householdRepository.GetByPropertyUnitIdAsync(
                        survey.PropertyUnitId.Value, cancellationToken);

                    result.Households = new List<HouseholdWithPersonsDto>();

                    foreach (var household in households)
                    {
                        // Map using CORRECT property names from Household entity
                        var householdDto = new HouseholdWithPersonsDto
                        {
                            Id = household.Id,
                            PropertyUnitId = household.PropertyUnitId,
                            HeadOfHouseholdName = household.HeadOfHouseholdName,
                            HeadOfHouseholdPersonId = household.HeadOfHouseholdPersonId,
                            HouseholdSize = household.HouseholdSize,
                            MaleCount = household.MaleCount,
                            FemaleCount = household.FemaleCount,
                            InfantCount = household.InfantCount,
                            ChildCount = household.ChildCount,
                            MinorCount = household.MinorCount,
                            AdultCount = household.AdultCount,
                            ElderlyCount = household.ElderlyCount,
                            PersonsWithDisabilitiesCount = household.PersonsWithDisabilitiesCount,
                            IsFemaleHeaded = household.IsFemaleHeaded,
                            IsDisplaced = household.IsDisplaced,
                            MonthlyIncomeEstimate = household.MonthlyIncomeEstimate,
                            Notes = household.Notes,
                            Persons = new List<PersonDto>()
                        };

                        // Get persons for this household
                        if (request.IncludePersons)
                        {
                            var persons = await _personRepository.GetByHouseholdIdAsync(household.Id, cancellationToken);
                            householdDto.Persons = _mapper.Map<List<PersonDto>>(persons);
                            result.DataSummary.PersonsCount += persons.Count;
                        }

                        result.Households.Add(householdDto);
                    }

                    result.DataSummary.HouseholdsCount = result.Households.Count;
                }

                // Get person-property relations for this property unit
                if (request.IncludeRelations)
                {
                    var relations = await _personPropertyRelationRepository.GetByPropertyUnitIdAsync(
                        survey.PropertyUnitId.Value, cancellationToken);

                    var relationsList = relations.ToList();
                    result.Relations = _mapper.Map<List<PersonPropertyRelationDto>>(relationsList);
                    result.DataSummary.RelationsCount = relationsList.Count;

                    // Count ownership relations
                    result.DataSummary.OwnershipRelationsCount = relationsList.Count(r =>
                        r.RelationType.Equals("Owner", StringComparison.OrdinalIgnoreCase) ||
                        r.RelationType.Equals("Heir", StringComparison.OrdinalIgnoreCase) ||
                        r.RelationType.Equals("Heirs", StringComparison.OrdinalIgnoreCase));
                }
            }
        }

        // Get evidence for this survey (by building context)
        // CORRECT signature: GetBySurveyContextAsync(buildingId, evidenceType?, cancellationToken)
        if (request.IncludeEvidence)
        {
            var evidence = await _evidenceRepository.GetBySurveyContextAsync(
                survey.BuildingId,
                null, // No filter on evidence type - get all types
                cancellationToken);

            result.Evidence = _mapper.Map<List<EvidenceDto>>(evidence);
            result.DataSummary.EvidenceCount = evidence.Count;
            result.DataSummary.TotalEvidenceSizeBytes = evidence.Sum(e => e.FileSizeBytes);
        }

        // Get linked claim if any
        if (survey.ClaimId.HasValue)
        {
            var claim = await _claimRepository.GetByIdAsync(survey.ClaimId.Value, cancellationToken);
            if (claim != null)
            {
                result.ClaimId = claim.Id;
                result.ClaimNumber = claim.ClaimNumber;
                result.ClaimStatus = claim.Status.ToString();
            }
        }

        return result;
    }
}