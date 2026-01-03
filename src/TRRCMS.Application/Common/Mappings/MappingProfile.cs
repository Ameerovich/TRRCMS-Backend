using AutoMapper;
using TRRCMS.Application.Buildings.Dtos;
using TRRCMS.Domain.Entities;

namespace TRRCMS.Application.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Building mappings
        CreateMap<Building, BuildingDto>()
            .ForMember(d => d.BuildingType, opt => opt.MapFrom(s => s.BuildingType.ToString()))
            .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.DamageLevel, opt => opt.MapFrom(s => s.DamageLevel.HasValue ? s.DamageLevel.ToString() : null));
    }
}