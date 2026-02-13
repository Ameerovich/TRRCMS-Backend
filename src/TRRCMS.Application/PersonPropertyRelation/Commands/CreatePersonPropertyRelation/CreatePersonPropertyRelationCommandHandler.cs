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

        var relation = Domain.Entities.PersonPropertyRelation.Create(
            request.PersonId,
            request.PropertyUnitId,
            request.RelationType,
            request.OccupancyType,
            request.HasEvidence,
            currentUserId);

        if (request.OwnershipShare.HasValue ||
            request.ContractDetails != null ||
            request.Notes != null)
        {
            relation.UpdateRelationDetails(
                request.RelationType,
                request.OccupancyType,
                request.HasEvidence,
                request.OwnershipShare,
                request.ContractDetails,
                request.Notes,
                currentUserId);
        }

        await _relationRepository.AddAsync(relation, cancellationToken);
        await _relationRepository.SaveChangesAsync(cancellationToken);

        var result = _mapper.Map<PersonPropertyRelationDto>(relation);
        result.IsOngoing = relation.IsActive;

        return result;
    }
}
