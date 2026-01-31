using AutoMapper;
using MediatR;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Surveys.Dtos;
using TRRCMS.Application.Households.Dtos;
using TRRCMS.Application.PersonPropertyRelations.Dtos;
using TRRCMS.Application.Evidences.Dtos;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Surveys.Queries.GetOfficeSurveyById;

/// <summary>
/// Handler for GetOfficeSurveyByIdQuery
/// </summary>
public class GetOfficeSurveyByIdQueryHandler : IRequestHandler<GetOfficeSurveyByIdQuery, OfficeSurveyDetailDto>
{
    private readonly ISurveyRepository _surveyRepository;
    private readonly IHouseholdRepository _householdRepository;
    private readonly IPersonPropertyRelationRepository _personPropertyRelationRepository;
    private readonly IPersonRepository _personRepository;
    private readonly IEvidenceRepository _evidenceRepository;
    private readonly IClaimRepository _claimRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public GetOfficeSurveyByIdQueryHandler(
        ISurveyRepository surveyRepository,
        IHouseholdRepository householdRepository,
        IPersonPropertyRelationRepository personPropertyRelationRepository,
        IPersonRepository personRepository,
        IEvidenceRepository evidenceRepository,
        IClaimRepository claimRepository,
        ICurrentUserService currentUserService,
        IMapper mapper)
    {
        _surveyRepository = surveyRepository ?? throw new ArgumentNullException(nameof(surveyRepository));
        _householdRepository = householdRepository ?? throw new ArgumentNullException(nameof(householdRepository));
        _personPropertyRelationRepository = personPropertyRelationRepository ?? throw new ArgumentNullException(nameof(personPropertyRelationRepository));
        _personRepository = personRepository ?? throw new ArgumentNullException(nameof(personRepository));
        _evidenceRepository = evidenceRepository ?? throw new ArgumentNullException(nameof(evidenceRepository));
        _claimRepository = claimRepository ?? throw new ArgumentNullException(nameof(claimRepository));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<OfficeSurveyDetailDto> Handle(
        GetOfficeSurveyByIdQuery request,
        CancellationToken cancellationToken)
    {
        var survey = await _surveyRepository.GetByIdAsync(request.SurveyId, cancellationToken)
            ?? throw new NotFoundException($"Survey with ID {request.SurveyId} not found");

        if (survey.Type != SurveyType.Office)
            throw new ValidationException("This endpoint is only for office surveys. Use field survey endpoints for field surveys.");

        var result = new OfficeSurveyDetailDto
        {
            Id = survey.Id,
            ReferenceCode = survey.ReferenceCode,
            BuildingId = survey.BuildingId,
            BuildingNumber = survey.Building?.BuildingNumber,
            BuildingAddress = survey.Building?.Address,
            PropertyUnitId = survey.PropertyUnitId,
            UnitIdentifier = survey.PropertyUnit?.UnitIdentifier,
            FieldCollectorId = survey.FieldCollectorId,
            FieldCollectorName = _currentUserService.Username,
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
            LastModifiedBy = survey.LastModifiedBy,
            OfficeLocation = survey.OfficeLocation,
            RegistrationNumber = survey.RegistrationNumber,
            AppointmentReference = survey.AppointmentReference,
            ContactPhone = survey.ContactPhone,
            ContactEmail = survey.ContactEmail,
            InPersonVisit = survey.InPersonVisit,
            ClaimId = survey.ClaimId,
            ClaimCreatedDate = survey.ClaimCreatedDate
        };

        if (survey.ClaimId.HasValue)
        {
            var claim = await _claimRepository.GetByIdAsync(survey.ClaimId.Value, cancellationToken);
            if (claim != null)
                result.ClaimNumber = claim.ClaimNumber;
        }

        if (survey.PropertyUnitId.HasValue)
        {
            var households = (await _householdRepository.GetByPropertyUnitIdAsync(survey.PropertyUnitId.Value, cancellationToken)).ToList();
            result.Households = _mapper.Map<List<HouseholdDto>>(households);

            var relations = (await _personPropertyRelationRepository.GetByPropertyUnitIdAsync(survey.PropertyUnitId.Value, cancellationToken)).ToList();
            result.Relations = _mapper.Map<List<PersonPropertyRelationDto>>(relations);

            var personCount = 0;
            foreach (var household in households)
            {
                var persons = await _personRepository.GetByHouseholdIdAsync(household.Id, cancellationToken);
                personCount += persons.Count;
            }

            // Count ownership relations using enum comparison
            var ownershipRelationsCount = relations.Count(r =>
                r.RelationType == RelationType.Owner ||
                r.RelationType == RelationType.Heir);

            // Get evidence using EvidenceType? enum (null = no filter)
            var evidence = await _evidenceRepository.GetBySurveyContextAsync(survey.BuildingId, null, cancellationToken);
            result.Evidence = _mapper.Map<List<EvidenceDto>>(evidence);

            result.DataSummary = new SurveyDataSummaryDto
            {
                PropertyUnitsCount = 1,
                HouseholdsCount = households.Count,
                PersonsCount = personCount,
                RelationsCount = relations.Count,
                OwnershipRelationsCount = ownershipRelationsCount,
                EvidenceCount = evidence.Count,
                TotalEvidenceSizeBytes = evidence.Sum(e => e.FileSizeBytes)
            };
        }
        else
        {
            result.DataSummary = new SurveyDataSummaryDto();
        }

        return result;
    }
}
