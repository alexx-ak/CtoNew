namespace VoxBox.Core.Common;

/// <summary>
/// Result wrapper for business operations - follows KISS and functional approach
/// </summary>
public class Result
{
    public bool IsSuccess { get; }
    public string? ErrorMessage { get; }
    
    protected Result(bool isSuccess, string? errorMessage)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
    }
    
    public static Result Success() => new(true, null);
    public static Result Failure(string message) => new(false, message);
    
    public static Result<T> Success<T>(T value) => new(true, null, value);
    public static Result<T> Failure<T>(string message) => new(false, message, default);
}

public class Result<T> : Result
{
    public T? Value { get; }
    
    protected internal Result(bool isSuccess, string? errorMessage, T? value) 
        : base(isSuccess, errorMessage)
    {
        Value = value;
    }
}
