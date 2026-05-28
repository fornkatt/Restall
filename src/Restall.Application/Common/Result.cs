namespace Restall.Application.Common;

/// <summary>
/// A custom Result type which can be used when you do not need to return a value. Auto-sets IsSuccess bool on Success or Error.
/// Use this type to easily return messages, errors and exceptions to propagate them for useful logging and user facing messages.
/// <br/><br/>
/// The messages returned by this type should be useful for logging and not be used to write messages to users. Instead, 
/// return an ErrorType with Result.Error and match on the ErrorType in a switch expression at UseCase level to write manual user-friendly messages.
/// </summary>
public sealed record Result(bool IsSuccess, string? ErrorMessage = null,
    Exception? Exception = null, ErrorType ErrorType = ErrorType.Unknown)
{
    public static Result Success() => new(true);
    public static Result Error(string? errorMessage, ErrorType errorType = ErrorType.Unknown,
        Exception? exception = null) => new(false, errorMessage, exception, errorType);
}

/// <summary>
/// A custom Result type which can be used as Result<![CDATA[<T>]]> to return a value. Auto-sets IsSuccess bool on Success or Error.
/// Use this type to easily return messages, errors and exceptions to propagate them for useful logging and user facing messages.
/// <br/><br/>
/// The messages returned by this type should be useful for logging and not be used to write messages to users. Instead, 
/// return a ErrorType with Result.Error and match on the ErrorType in a switch expression at UseCase level to write manual user-friendly messages.
/// </summary>
public sealed record Result<T>(bool IsSuccess, T? Value = default, string? ErrorMessage = null,
    Exception? Exception = null,  ErrorType ErrorType = ErrorType.Unknown)
{
    public static Result<T> Success(T value) => new(true, value);
    public static Result<T> Error(string? errorMessage, ErrorType errorType = ErrorType.Unknown, 
        Exception? exception = null) =>
        new(false, ErrorMessage: errorMessage, Exception: exception, ErrorType: errorType);
}

public enum ErrorType
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