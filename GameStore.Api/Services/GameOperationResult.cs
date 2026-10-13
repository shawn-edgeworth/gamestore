namespace GameStore.Api.Services;

public struct OperationResult<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? Error { get; }
    public OperationErrorType ErrorType { get; }

    private OperationResult(bool isSuccess, T? value, string? error,
    OperationErrorType errorType)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
        ErrorType = errorType;
    }

    public static OperationResult<T> Success(T value)
    {
        return new OperationResult<T>(true, value, null, OperationErrorType.None);
    }

    public static OperationResult<T> Failure(string error, OperationErrorType errorType)
    {
        return new OperationResult<T>(false, default, error, errorType);
    }
}