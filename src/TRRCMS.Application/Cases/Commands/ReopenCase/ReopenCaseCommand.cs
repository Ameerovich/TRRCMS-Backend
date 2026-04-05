using MediatR;

namespace TRRCMS.Application.Cases.Commands.ReopenCase;

public record ReopenCaseCommand(Guid CaseId) : IRequest<Unit>;
