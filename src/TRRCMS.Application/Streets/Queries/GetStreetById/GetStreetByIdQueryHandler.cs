using AutoMapper;
using MediatR;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Streets.Dtos;

namespace TRRCMS.Application.Streets.Queries.GetStreetById;

public class GetStreetByIdQueryHandler : IRequestHandler<GetStreetByIdQuery, StreetDto>
{
    private readonly IStreetRepository _streetRepository;
    private readonly IMapper _mapper;

    public GetStreetByIdQueryHandler(
        IStreetRepository streetRepository,
        IMapper mapper)
    {
        _streetRepository = streetRepository;
        _mapper = mapper;
    }

    public async Task<StreetDto> Handle(GetStreetByIdQuery request, CancellationToken cancellationToken)
    {
        var street = await _streetRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException($"Street with ID {request.Id} not found.");

        return _mapper.Map<StreetDto>(street);
    }
}
