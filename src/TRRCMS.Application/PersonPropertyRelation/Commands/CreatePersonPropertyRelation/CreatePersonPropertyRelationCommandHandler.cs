using AutoMapper;
using MediatR;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.PersonPropertyRelations.Dtos;
using TRRCMS.Domain.Entities;

namespace TRRCMS.Application.PersonPropertyRelations.Commands.CreatePersonPropertyRelation;

/// <summary>
/// Handler for CreatePersonPropertyRelationCommand
/// </summary>
public class CreatePersonPropertyRelationCommandHandler : IRequestHandler<CreatePersonPropertyRelationCommand, PersonPropertyRelationDto>
{
    private readonly IPersonPropertyRelationRepository _relationRepository;
    private readonly IMapper _mapper;

    public CreatePersonPropertyRelationCommandHandler(
        IPersonPropertyRelationRepository relationRepository,
        IMapper mapper)
    {
        _relationRepository = relationRepository;
        _mapper = mapper;
    }

    public async Task<PersonPropertyRelationDto> Handle(CreatePersonPropertyRelationCommand request, CancellationToken cancellationToken)
    {
        // Validate required fields
        if (request.PersonId == Guid.Empty)
            throw new ArgumentException("PersonId is required", nameof(request.PersonId));

        if (request.PropertyUnitId == Guid.Empty)
            throw new ArgumentException("PropertyUnitId is required", nameof(request.PropertyUnitId));

        if (string.IsNullOrWhiteSpace(request.RelationType))
            throw new ArgumentException("RelationType is required", nameof(request.RelationType));

        if (request.CreatedByUserId == Guid.Empty)
            throw new ArgumentException("CreatedByUserId is required", nameof(request.CreatedByUserId));

        // Create relation using factory method
        var relation = PersonPropertyRelation.Create(
            request.PersonId,
            request.PropertyUnitId,
            request.RelationType,
            request.CreatedByUserId);

        // Convert DateTime values to UTC if they have a value
        var startDateUtc = request.StartDate.HasValue
            ? DateTime.SpecifyKind(request.StartDate.Value, DateTimeKind.Utc)
            : (DateTime?)null;

        var endDateUtc = request.EndDate.HasValue
            ? DateTime.SpecifyKind(request.EndDate.Value, DateTimeKind.Utc)
            : (DateTime?)null;

        // Update optional details if provided
        if (request.OwnershipShare.HasValue ||
            !string.IsNullOrWhiteSpace(request.ContractDetails) ||
            !string.IsNullOrWhiteSpace(request.RelationTypeOtherDesc) ||
            startDateUtc.HasValue ||
            endDateUtc.HasValue ||
            !string.IsNullOrWhiteSpace(request.Notes))
        {
            relation.UpdateRelationDetails(
                request.RelationType,
                request.RelationTypeOtherDesc,
                request.OwnershipShare,
                request.ContractDetails,
                startDateUtc,  // Use UTC version
                endDateUtc,    // Use UTC version
                request.Notes,
                request.CreatedByUserId);
        }

        // Add to repository
        await _relationRepository.AddAsync(relation, cancellationToken);
        await _relationRepository.SaveChangesAsync(cancellationToken);

        // Map to DTO
        return _mapper.Map<PersonPropertyRelationDto>(relation);
    }
}