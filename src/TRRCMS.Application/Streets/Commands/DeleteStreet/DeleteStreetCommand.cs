using MediatR;

namespace TRRCMS.Application.Streets.Commands.DeleteStreet;

/// <summary>
/// Soft-delete a street.
/// حذف شارع
/// </summary>
public record DeleteStreetCommand : IRequest<Unit>
{
    /// <summary>Street ID (GUID)</summary>
    public Guid Id { get; init; }
}
