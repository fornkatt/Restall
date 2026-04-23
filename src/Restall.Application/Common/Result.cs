namespace Restall.Application.Common;

/// <summary>
/// A custom Result type which can be used when you do not need to return a value. Auto-sets IsSuccess bool on Ok or Err.
/// Use this type to easily return messages, errors and exceptions to propagate them for useful logging and user facing messages.
/// <br/><br/>
/// The messages returned by this type should be useful for logging and not be used to write messages to users. Instead, 
/// return a ResultError with Result.Err and match on the ResultError in a switch expression at UseCase level to write manual user-friendly messages.
/// </summary>
public sealed record Result(bool IsSuccess, string? ErrorMessage = null,
    Exception? Exception = null, ResultError Error = ResultError.Unknown)
{
    public static Result Ok() => new(true);
    public static Result Err(string? errorMessage, ResultError error = ResultError.Unknown,
        Exception? exception = null) => new(false, errorMessage, exception, error);
}

/// <summary>
/// A custom Result type which can be used as Result<![CDATA[<T>]]> to return a value. Auto-sets IsSuccess bool on Ok or Err.
/// Use this type to easily return messages, errors and exceptions to propagate them for useful logging and user facing messages.
/// <br/><br/>
/// The messages returned by this type should be useful for logging and not be used to write messages to users. Instead, 
/// return a ResultError with Result.Err and match on the ResultError in a switch expression at UseCase level to write manual user-friendly messages.
/// </summary>
public sealed record Result<T>(bool IsSuccess, T? Value = default, string? ErrorMessage = null,
    Exception? Exception = null,  ResultError Error = ResultError.Unknown)
{
    public static Result<T> Ok(T value) => new(true, value);
    public static Result<T> Err(string? errorMessage, ResultError error = ResultError.Unknown, 
        Exception? exception = null) =>
        new(false, ErrorMessage: errorMessage, Exception: exception, Error: error);
}

public enum ResultError
{
    Unknown,
    
    // File extraction
    ToolNotFound,
    ExtractionFailed,
    ProcessStartFailed,
    
    // Filesystem
    PermissionDenied,
    FileSystemError,
    
    //Network
    DownloadFailed,
    NetworkTimeout,
    
    // Install
    FileNotFound,
}