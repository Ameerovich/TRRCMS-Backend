using MediatR;
using TRRCMS.Application.Households.Dtos;

namespace TRRCMS.Application.Households.Commands.UpdateHousehold;

/// <summary>
/// Command to update a household — partial update (canonical v1.9 shape).
/// Only provided fields are changed; nulls mean "don't update".
/// </summary>
public class UpdateHouseholdCommand : IRequest<HouseholdDto>
{
    /// <summary>Household ID to update (required)</summary>
    public Guid Id { get; set; }

    public int? HouseholdSize { get; set; }
    public string? Notes { get; set; }

    public int? MaleCount { get; set; }
    public int? FemaleCount { get; set; }
    public int? AdultCount { get; set; }
    public int? ChildCount { get; set; }
    public int? ElderlyCount { get; set; }
    public int? DisabledCount { get; set; }

    /// <summary>Occupancy nature enum code</summary>
    public int? OccupancyNature { get; set; }

    /// <summary>Occupancy start date (UTC)</summary>
    public DateTime? OccupancyStartDate { get; set; }
}
