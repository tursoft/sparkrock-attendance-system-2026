using System.Diagnostics.CodeAnalysis;

namespace AttendanceSystem.Application.Common;

/// <summary>The outcome of a use case: success, or exactly one <see cref="Common.Error"/>.</summary>
public class Result
{
    private static readonly Result SuccessResult = new(null);

    private protected Result(Error? error) => Error = error;

    public Error? Error { get; }

    [MemberNotNullWhen(false, nameof(Error))]
    public bool IsSuccess => Error is null;

    [MemberNotNullWhen(true, nameof(Error))]
    public bool IsFailure => Error is not null;

    public static Result Success() => SuccessResult;

    public static Result Failure(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);
        return new(error);
    }

    public static Result<T> Success<T>(T value) => new(value);

    public static Result<T> Failure<T>(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);
        return new(error);
    }

    public static implicit operator Result(Error error) => Failure(error);
}

/// <summary>The outcome of a use case that produces a value on success.</summary>
public sealed class Result<T> : Result
{
    private readonly T _value;

    internal Result(T value)
        : base(null) => _value = value;

    internal Result(Error error)
        : base(error) => _value = default!;

    /// <summary>The value of a successful result. Reading it from a failure is a programming error.</summary>
    public T Value => IsSuccess
        ? _value
        : throw new InvalidOperationException("A failed result has no value; check IsSuccess first.");

    public static implicit operator Result<T>(T value) => new(value);

    public static implicit operator Result<T>(Error error) => Failure<T>(error);
}
