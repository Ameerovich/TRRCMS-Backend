using MediatR;
using TRRCMS.Application.Cases.Dtos;

namespace TRRCMS.Application.Cases.Queries.GetCaseByPropertyUnit;

public record GetCaseByPropertyUnitQuery(Guid PropertyUnitId) : IRequest<CaseDto?>;
