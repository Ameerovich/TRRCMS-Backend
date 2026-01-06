using AutoMapper;
using TRRCMS.Application.Buildings.Dtos;
using TRRCMS.Application.Households.Dtos;
using TRRCMS.Application.Persons.Dtos;
using TRRCMS.Application.PropertyUnits.Dtos;
using TRRCMS.Domain.Entities;

namespace TRRCMS.Application.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Building mappings
        CreateMap<Building, BuildingDto>()
            .ForMember(dest => dest.BuildingType, opt => opt.MapFrom(src => src.BuildingType.ToString()))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.DamageLevel, opt => opt.MapFrom(src => src.DamageLevel.HasValue ? src.DamageLevel.ToString() : null));

        // PropertyUnit mappings
        CreateMap<PropertyUnit, PropertyUnitDto>()
            .ForMember(dest => dest.UnitType, opt => opt.MapFrom(src => src.UnitType.ToString()))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.DamageLevel, opt => opt.MapFrom(src => src.DamageLevel.HasValue ? src.DamageLevel.ToString() : null))
            .ForMember(dest => dest.OccupancyType, opt => opt.MapFrom(src => src.OccupancyType.HasValue ? src.OccupancyType.ToString() : null))
            .ForMember(dest => dest.OccupancyNature, opt => opt.MapFrom(src => src.OccupancyNature.HasValue ? src.OccupancyNature.ToString() : null));
      
        // Person mappings
        CreateMap<Person, PersonDto>();

        // Household mappings
        CreateMap<Household, HouseholdDto>()
            .ForMember(dest => dest.DependencyRatio, opt => opt.MapFrom(src => src.CalculateDependencyRatio()))
            .ForMember(dest => dest.IsVulnerable, opt => opt.MapFrom(src => src.IsVulnerable()));

    }

}