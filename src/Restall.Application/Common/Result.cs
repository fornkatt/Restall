// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Restall.Application.Common;

/// <summary>
/// A custom Result type that can be used when you do not need to return a value. Auto-sets IsSuccess bool on Success or Error.
/// Use this type to easily return messages, errors, and exceptions to propagate them for useful logging and user-facing messages.
/// </summary>
/// <remarks>
///     <para>
/// The messages returned by this type should be useful for logging and not be used to write messages to users. Instead,
/// return an ErrorType with Result.Error and match on the ErrorType in a switch expression to write manual user-friendly messages.
///     </para>
/// </remarks>
public sealed record Result(
    bool IsSuccess,
    string? ErrorMessage = null,
    Exception? Exception = null,
    ErrorType ErrorType = ErrorType.None)
{
    public static Result Success() => new(true);

    /// <param name="errorMessage">An optional error message.</param>
    /// <param name="errorType">An optional error type defined in the ErrorType enum.</param>
    /// <param name="exception">An optional exception.</param>
    public static Result Error(string? errorMessage, ErrorType errorType = ErrorType.None,
        Exception? exception = null) => new(false, errorMessage, exception, errorType);
}

/// <summary>
/// A custom Result type that can be used as Result<![CDATA[<T>]]> to return a value. Auto-sets IsSuccess bool on Success or Error.
/// Use this type to easily return messages, errors, and exceptions to propagate them for useful logging and user-facing messages.
/// </summary>
/// <remarks>
///     <para>
/// The messages returned by this type should be useful for logging and not be used to write messages to users. Instead,
/// return an ErrorType with Result.Error and match on the ErrorType in a switch expression to write manual user-friendly messages.
///     </para>
/// </remarks>
public sealed record Result<T>(
    bool IsSuccess,
    T? Value = default,
    string? ErrorMessage = null,
    Exception? Exception = null,
    ErrorType ErrorType = ErrorType.None)
{
    /// <param name="value">T value to return with the success status.</param>
    public static Result<T> Success(T value) => new(true, value);

    /// <param name="errorMessage">An optional error message.</param>
    /// <param name="errorType">An optional error type defined in the ErrorType enum.</param>
    /// <param name="exception">An optional exception.</param>
    public static Result<T> Error(string? errorMessage, ErrorType errorType = ErrorType.None,
        Exception? exception = null) =>
        new(false, ErrorMessage: errorMessage, Exception: exception, ErrorType: errorType);
}

public enum ErrorType
{
    None,

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
