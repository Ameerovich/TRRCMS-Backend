using AutoMapper;
using MediatR;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.PersonPropertyRelations.Dtos;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.PersonPropertyRelations.Commands.CreatePersonPropertyRelation;

public class CreatePersonPropertyRelationCommandHandler : IRequestHandler<CreatePersonPropertyRelationCommand, PersonPropertyRelationDto>
{
    private readonly IPersonPropertyRelationRepository _relationRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public CreatePersonPropertyRelationCommandHandler(
        IPersonPropertyRelationRepository relationRepository,
        ICurrentUserService currentUserService,
        IMapper mapper)
    {
        _relationRepository = relationRepository;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<PersonPropertyRelationDto> Handle(CreatePersonPropertyRelationCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated");

        if (request.RelationType == RelationType.Other && string.IsNullOrWhiteSpace(request.RelationTypeOtherDesc))
            throw new ArgumentException("Description is required when relation type is 'Other'", nameof(request.RelationTypeOtherDesc));

        if (request.ContractType == TenureContractType.Other && string.IsNullOrWhiteSpace(request.ContractTypeOtherDesc))
            throw new ArgumentException("Description is required when contract type is 'Other'", nameof(request.ContractTypeOtherDesc));

        var relation = Domain.Entities.PersonPropertyRelation.Create(
            request.PersonId,
            request.PropertyUnitId,
            request.RelationType,
            currentUserId);

        if (request.RelationTypeOtherDesc != null ||
            request.ContractType.HasValue ||
            request.OwnershipShare.HasValue ||
            request.ContractDetails != null ||
            request.StartDate.HasValue ||
            request.EndDate.HasValue ||
            request.Notes != null)
        {
            relation.UpdateRelationDetails(
                request.RelationType,
                request.RelationTypeOtherDesc,
                request.ContractType,
                request.ContractTypeOtherDesc,
                request.OwnershipShare,
                request.ContractDetails,
                request.StartDate,
                request.EndDate,
                request.Notes,
                currentUserId);
        }

        await _relationRepository.AddAsync(relation, cancellationToken);
        await _relationRepository.SaveChangesAsync(cancellationToken);

        var result = _mapper.Map<PersonPropertyRelationDto>(relation);

        // Calculate computed properties
        if (relation.StartDate.HasValue)
        {
            if (relation.EndDate.HasValue)
            {
                result.DurationInDays = (int)(relation.EndDate.Value - relation.StartDate.Value).TotalDays;
                result.IsOngoing = false;
            }
            else
            {
                result.DurationInDays = (int)(DateTime.UtcNow - relation.StartDate.Value).TotalDays;
                result.IsOngoing = true;
            }
        }

        return result;
    }
}
