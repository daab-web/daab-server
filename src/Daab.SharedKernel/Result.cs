namespace Daab.SharedKernel;

public record Error(int Code, string Message)
{
    public static Error New(int code, string message) => new(code, message);
}

public sealed class Result<TValue>
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;

    private readonly TValue? _value;
    private readonly Error? _error;

    public TValue Value =>
        IsSuccess
            ? _value!
            : throw new InvalidOperationException("Cannot access Value on a failed result.");

    public Error Error =>
        IsFailure
            ? _error!
            : throw new InvalidOperationException("Cannot access Error on a successful result.");

    private Result(TValue value)
    {
        ArgumentNullException.ThrowIfNull(value, nameof(value));
        IsSuccess = true;
        _value = value;
    }

    private Result(Error error)
    {
        ArgumentNullException.ThrowIfNull(error, nameof(error));
        IsSuccess = false;
        _error = error;
    }

    public TResult Match<TResult>(Func<TValue, TResult> success, Func<Error, TResult> error) =>
        IsSuccess ? success(_value!) : error(_error!);

    public static Result<TValue> Success(TValue value) => new(value);

    public static Result<TValue> Failure(Error error) => new(error);

    public static implicit operator Result<TValue>(TValue value) => new(value);

    public static implicit operator Result<TValue>(Error error) => new(error);
}

