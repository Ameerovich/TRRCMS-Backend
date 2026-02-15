using FluentValidation;
using MediatR;
using TRRCMS.Application.Common.Exceptions;
using ValidationException = TRRCMS.Application.Common.Exceptions.ValidationException;

namespace TRRCMS.Application.Common.Behaviors;

/// <summary>
/// MediatR pipeline behavior that validates commands/queries using FluentValidation.
/// Runs before the command handler executes.
/// Throws <see cref="ValidationException"/> with field-level errors when validation fails.
/// </summary>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // If no validators registered for this request type, skip validation
        if (!_validators.Any())
        {
            return await next();
        }

        // Create validation context
        var context = new ValidationContext<TRequest>(request);

        // Run all validators
        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        // Collect all validation failures
        var failures = validationResults
            .SelectMany(result => result.Errors)
            .Where(failure => failure != null)
            .ToList();

        // If there are validation errors, throw our custom ValidationException
        if (failures.Any())
        {
            throw new ValidationException(failures);
        }

        // Validation passed, continue to handler
        return await next();
    }
}