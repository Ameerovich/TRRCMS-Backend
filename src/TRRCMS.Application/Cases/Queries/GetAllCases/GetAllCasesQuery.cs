using MediatR;
using TRRCMS.Application.Cases.Dtos;
using TRRCMS.Application.Common.Models;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Cases.Queries.GetAllCases;

public record GetAllCasesQuery : IRequest<ListResponse<CaseSummaryDto>>
{
    public CaseLifecycleStatus? Status { get; init; }
    public Guid? BuildingId { get; init; }

    /// <summary>
    /// Filter by 17-digit building code (with or without dashes). Overrides BuildingId if both provided.
    /// </summary>
    public string? BuildingCode { get; init; }

    /// <summary>
    /// Filter by property unit identifier within the building (e.g., "Apt 1"). Requires BuildingCode or BuildingId.
    /// </summary>
    public string? UnitIdentifier { get; init; }

    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
