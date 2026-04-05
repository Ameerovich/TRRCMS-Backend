using MediatR;
using TRRCMS.Application.Cases.Dtos;

namespace TRRCMS.Application.Cases.Queries.GetCase;

public record GetCaseQuery(Guid CaseId) : IRequest<CaseDto>;
