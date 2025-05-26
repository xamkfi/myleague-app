using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Application.Common;

namespace Application.Behaviors;

public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : class, IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    private static readonly ConcurrentDictionary<Type, MethodInfo> _validationFailureMethods
        = new ConcurrentDictionary<Type, MethodInfo>();

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Respect cancellation ASAP
        cancellationToken.ThrowIfCancellationRequested();

        if (!_validators.Any())
            return await next();

        ValidationContext<TRequest> context = new ValidationContext<TRequest>(request);
        ValidationResult[] validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        List<ValidationFailure> failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Any())
            return CreateValidationFailureResult(failures);

        return await next();
    }

    private static TResponse CreateValidationFailureResult(List<ValidationFailure> failures)
    {
        Type responseType = typeof(TResponse);

        // Handle generic Result<T>
        if (responseType.IsGenericType &&
            responseType.GetGenericTypeDefinition() == typeof(Result<>))
        {
            MethodInfo method = _validationFailureMethods.GetOrAdd(responseType, rt =>
            {
                Type arg = rt.GetGenericArguments()[0];
                Type resultType = typeof(Result<>).MakeGenericType(arg);
                return resultType.GetMethod(
                    nameof(Result<object>.ValidationFailure),
                    new[] { typeof(IEnumerable<ValidationFailure>) }
                )!;
            });

            return (TResponse)method.Invoke(null, new object[] { failures })!;
        }

        // Handle non-generic Result
        if (responseType == typeof(Result))
        {
            return (TResponse)(object)Result.ValidationFailure(failures);
        }

        // Fallback: throw a ValidationException with the original ValidationFailure objects
        // This preserves all detailed error information
        throw new ValidationException(failures);
    }
}
