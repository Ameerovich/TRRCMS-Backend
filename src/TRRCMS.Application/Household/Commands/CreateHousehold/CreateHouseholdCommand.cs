using MediatR;
using TRRCMS.Application.Households.Dtos;

namespace TRRCMS.Application.Households.Commands.CreateHousehold;

/// <summary>
/// Command to create a new household (canonical v1.9 shape).
/// </summary>
public class CreateHouseholdCommand : IRequest<HouseholdDto>
{
    /// <summary>Property unit ID this household belongs to (required)</summary>
    public Guid PropertyUnitId { get; set; }

    /// <summary>Total household size (عدد الأفراد) — required, 1–50</summary>
    public int HouseholdSize { get; set; }

    /// <summary>Total males across all ages (عدد الذكور)</summary>
    public int? MaleCount { get; set; }

    /// <summary>Total females across all ages (عدد الإناث)</summary>
    public int? FemaleCount { get; set; }

    /// <summary>Number of adults (عدد البالغين)</summary>
    public int? AdultCount { get; set; }

    /// <summary>Number of children (عدد الأطفال)</summary>
    public int? ChildCount { get; set; }

    /// <summary>Number of elderly (عدد كبار السن)</summary>
    public int? ElderlyCount { get; set; }

    /// <summary>Number of persons with disabilities (عدد ذوي الإعاقة)</summary>
    public int? DisabledCount { get; set; }

    /// <summary>Occupancy nature enum code (optional)</summary>
    public int? OccupancyNature { get; set; }

    /// <summary>Date the household started occupying this unit (UTC)</summary>
    public DateTime? OccupancyStartDate { get; set; }

    /// <summary>Notes/observations (ملاحظات)</summary>
    public string? Notes { get; set; }
}
