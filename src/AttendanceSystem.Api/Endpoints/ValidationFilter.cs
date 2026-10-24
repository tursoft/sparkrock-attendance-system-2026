using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;

namespace AttendanceSystem.Api.Endpoints;

/// <summary>
/// Validates the DataAnnotations of the bound <typeparamref name="TRequest"/>, including nested objects and the items of
/// collection properties, and answers 400 ValidationProblem before the handler runs. Error keys use the JSON names
/// clients send (<c>entries[1].studentId</c>), the same way application validation errors are keyed.
/// </summary>
public sealed class ValidationFilter<TRequest>(IOptions<JsonOptions> jsonOptions) : IEndpointFilter
    where TRequest : class
{
    private readonly JsonNamingPolicy? _namingPolicy = jsonOptions.Value.SerializerOptions.PropertyNamingPolicy;

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        var request = context.Arguments.OfType<TRequest>().FirstOrDefault();
        if (request is null)
        {
            return await next(context);
        }

        var errors = new Dictionary<string, List<string>>(StringComparer.Ordinal);
        ValidateObject(request, string.Empty, errors, new HashSet<object>(ReferenceEqualityComparer.Instance));

        return errors.Count == 0
            ? await next(context)
            // TypedResults.Problem goes through IProblemDetailsService (adds traceId); ValidationProblem doesn't in .NET 8.
            : TypedResults.Problem(new HttpValidationProblemDetails(
                errors.ToDictionary(error => error.Key, error => error.Value.ToArray(), StringComparer.Ordinal))
            {
                Status = StatusCodes.Status400BadRequest,
            });
    }

    private void ValidateObject(object instance, string prefix, Dictionary<string, List<string>> errors, HashSet<object> visited)
    {
        if (!visited.Add(instance))
        {
            return;
        }

        var results = new List<ValidationResult>();
        Validator.TryValidateObject(instance, new ValidationContext(instance), results, validateAllProperties: true);
        foreach (var result in results)
        {
            var members = result.MemberNames.Any() ? result.MemberNames : [string.Empty];
            foreach (var member in members)
            {
                AddError(errors, Key(prefix, member), result.ErrorMessage ?? "The value is invalid.");
            }
        }

        foreach (var property in instance.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (property.GetIndexParameters().Length > 0 || !IsComplex(property.PropertyType))
            {
                continue;
            }

            var value = property.GetValue(instance);
            var path = Key(prefix, property.Name);
            if (value is IEnumerable items)
            {
                ValidateItems(items, path, errors, visited);
            }
            else if (value is not null)
            {
                ValidateObject(value, path + ".", errors, visited);
            }
        }
    }

    private void ValidateItems(IEnumerable items, string path, Dictionary<string, List<string>> errors, HashSet<object> visited)
    {
        var index = 0;
        foreach (var item in items)
        {
            if (item is not null && IsComplex(item.GetType()))
            {
                ValidateObject(item, $"{path}[{index}].", errors, visited);
            }

            index++;
        }
    }

    private string Key(string prefix, string member) =>
        member.Length == 0 ? prefix.TrimEnd('.') : prefix + (_namingPolicy?.ConvertName(member) ?? member);

    private static void AddError(Dictionary<string, List<string>> errors, string key, string message)
    {
        if (!errors.TryGetValue(key, out var messages))
        {
            messages = [];
            errors[key] = messages;
        }

        messages.Add(message);
    }

    // Value types and strings carry no nested annotations; framework types are not request DTOs.
    private static bool IsComplex(Type type) =>
        !type.IsValueType && type != typeof(string) && (typeof(IEnumerable).IsAssignableFrom(type) || type.Namespace?.StartsWith("System", StringComparison.Ordinal) != true);
}

public static class ValidationEndpointExtensions
{
    /// <summary>Validates the bound <typeparamref name="TRequest"/> with <see cref="ValidationFilter{TRequest}"/> and documents the 400 response.</summary>
    public static RouteHandlerBuilder WithValidation<TRequest>(this RouteHandlerBuilder builder)
        where TRequest : class
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.AddEndpointFilter<ValidationFilter<TRequest>>().ProducesValidationProblem();
    }
}
