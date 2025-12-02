using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Common.Pipeline;

public sealed class ValidationBehavior(IServiceProvider sp) : IRequestBehavior
{
    public async Task OnExecutingAsync(object request, CancellationToken ct = default)
    {
        var validatorType = typeof(IValidator<>).MakeGenericType(request.GetType());
        var validators = sp.GetServices(validatorType).Cast<object>().ToList();
        if (validators.Count == 0) return;
        var failures = new List<FluentValidation.Results.ValidationFailure>();
        foreach (var v in validators)
        {
            var method = validatorType.GetMethod("ValidateAsync", [request.GetType(), typeof(CancellationToken)])
            ?? validatorType.GetMethod("Validate", [request.GetType()]);
            if (method is null) continue;
            var resultObj = method.GetParameters().Length == 2
            ? await (Task<FluentValidation.Results.ValidationResult>)method.Invoke(v, new object?[] { request, ct })!
            : (FluentValidation.Results.ValidationResult)method.Invoke(v, new object?[] { request })!;
            failures.AddRange(resultObj.Errors.Where(e => e is not null));
        }
        if (failures.Count > 0)
        {
            var messages = string.Join("; ", failures.Select(f => f.ErrorMessage));
            throw new ValidationException(messages, failures);
        }
    }

    public Task OnExecutedAsync(object request, object? response, Exception? exception, TimeSpan elapsed, CancellationToken ct = default)
    => Task.CompletedTask;
}
