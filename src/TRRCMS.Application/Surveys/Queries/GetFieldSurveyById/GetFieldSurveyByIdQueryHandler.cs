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
        var survey = await _surveyRepository.GetByIdAsync(request.SurveyId, cancellationToken)
            ?? throw new NotFoundException($"Survey with ID {request.SurveyId} not found");

        if (survey.Type != SurveyType.Field)
            throw new ValidationException("This endpoint is only for field surveys. Use /api/surveys/office/{id} for office surveys.");

        var result = new FieldSurveyDetailDto
        {
            Id = survey.Id,
            ReferenceCode = survey.ReferenceCode,
            BuildingId = survey.BuildingId,
            PropertyUnitId = survey.PropertyUnitId,
            FieldCollectorId = survey.FieldCollectorId,
            SurveyDate = survey.SurveyDate,
            Status = (int)survey.Status,
            SurveyType = (int)survey.Type,
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

        if (survey.Building != null)
        {
            result.BuildingNumber = survey.Building.BuildingNumber;
            result.BuildingAddress = survey.Building.Address;
        }

        var fieldCollector = await _userRepository.GetByIdAsync(survey.FieldCollectorId, cancellationToken);
        if (fieldCollector != null)
            result.FieldCollectorName = fieldCollector.FullNameArabic ?? fieldCollector.Username;

        var creator = await _userRepository.GetByIdAsync(survey.CreatedBy, cancellationToken);
        if (creator != null)
            result.CreatedByName = creator.FullNameArabic ?? creator.Username;

        result.DataSummary = new SurveyDataSummaryDto();

        if (survey.PropertyUnitId.HasValue)
        {
            var propertyUnit = await _propertyUnitRepository.GetByIdAsync(survey.PropertyUnitId.Value, cancellationToken);
            if (propertyUnit != null)
            {
                result.UnitIdentifier = propertyUnit.UnitIdentifier;
                result.PropertyUnit = _mapper.Map<PropertyUnitDto>(propertyUnit);
                result.DataSummary.PropertyUnitsCount = 1;

                if (request.IncludeHouseholds)
                {
                    var households = await _householdRepository.GetByPropertyUnitIdAsync(survey.PropertyUnitId.Value, cancellationToken);
                    result.Households = new List<HouseholdWithPersonsDto>();

                    foreach (var household in households)
                    {
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

                if (request.IncludeRelations)
                {
                    var relations = await _personPropertyRelationRepository.GetByPropertyUnitIdAsync(survey.PropertyUnitId.Value, cancellationToken);
                    var relationsList = relations.ToList();
                    result.Relations = _mapper.Map<List<PersonPropertyRelationDto>>(relationsList);
                    result.DataSummary.RelationsCount = relationsList.Count;

                    // Count ownership relations using enum comparison
                    result.DataSummary.OwnershipRelationsCount = relationsList.Count(r =>
                        r.RelationType == RelationType.Owner ||
                        r.RelationType == RelationType.Heir);
                }
            }
        }

        if (request.IncludeEvidence)
        {
            // Get evidence using EvidenceType? enum (null = no filter)
            var evidence = await _evidenceRepository.GetBySurveyContextAsync(survey.BuildingId, null, cancellationToken);
            result.Evidence = _mapper.Map<List<EvidenceDto>>(evidence);
            result.DataSummary.EvidenceCount = evidence.Count;
            result.DataSummary.TotalEvidenceSizeBytes = evidence.Sum(e => e.FileSizeBytes);
        }

        if (survey.ClaimId.HasValue)
        {
            var claim = await _claimRepository.GetByIdAsync(survey.ClaimId.Value, cancellationToken);
            if (claim != null)
            {
                result.ClaimId = claim.Id;
                result.ClaimNumber = claim.ClaimNumber;
                result.ClaimStatus = (int)claim.Status;
            }
        }

        return result;
    }
}
