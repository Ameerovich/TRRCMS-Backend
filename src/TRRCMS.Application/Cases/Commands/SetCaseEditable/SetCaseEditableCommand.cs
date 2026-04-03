using MediatR;

namespace TRRCMS.Application.Cases.Commands.SetCaseEditable;

public record SetCaseEditableCommand(Guid CaseId, bool IsEditable) : IRequest<Unit>;
