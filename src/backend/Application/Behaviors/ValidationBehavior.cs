using FluentValidation;
using MediatR;
using Application.Common;

namespace Application.Behaviors;

/// <summary>
/// Pipeline behavior for automatic validation
/// </summary>
/// <typeparam name="TRequest">The request type</typeparam>
/// <typeparam name="TResponse">The response type</typeparam>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : class, IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        ValidationContext<TRequest> context = new ValidationContext<TRequest>(request);

        FluentValidation.Results.ValidationResult[] validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        List<string> failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .Select(f => f.ErrorMessage)
            .ToList();

        if (failures.Any())
        {
            // Check if TResponse is a Result type
            if (typeof(TResponse).IsGenericType && typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
            {
                // Create a validation failure result
                Type resultType = typeof(TResponse).GetGenericArguments()[0];
                Type genericResultType = typeof(Result<>).MakeGenericType(resultType);
                System.Reflection.MethodInfo? method = genericResultType.GetMethod(nameof(Result<object>.ValidationFailure));
                object? result = method?.Invoke(null, new object[] { failures });
                return (TResponse)result!;
            }
            else if (typeof(TResponse) == typeof(Result))
            {
                // Create a validation failure result for non-generic Result
                Result result = Result.ValidationFailure(failures);
                return (TResponse)(object)result;
            }
            else
            {
                // Fallback to throwing exception for non-Result types
                throw new ValidationException(failures.Select(f => new FluentValidation.Results.ValidationFailure("", f)));
            }
        }

        return await next();
    }
} 