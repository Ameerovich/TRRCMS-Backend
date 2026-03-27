using AutoMapper;
using MediatR;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Landmarks.Dtos;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Landmarks.Commands.UpdateLandmarkTypeIcon;

public class UpdateLandmarkTypeIconCommandHandler : IRequestHandler<UpdateLandmarkTypeIconCommand, LandmarkTypeIconDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public UpdateLandmarkTypeIconCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<LandmarkTypeIconDto> Handle(UpdateLandmarkTypeIconCommand request, CancellationToken cancellationToken)
    {
        var landmarkType = (LandmarkType)request.Type;

        var icon = await _unitOfWork.LandmarkTypeIcons.GetByTypeAsync(landmarkType, cancellationToken)
            ?? throw new NotFoundException($"Landmark type icon for type {request.Type} not found.");

        icon.UpdateIcon(request.SvgContent, _currentUserService.UserId ?? Guid.Empty);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<LandmarkTypeIconDto>(icon);
    }
}
