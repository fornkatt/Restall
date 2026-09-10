// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Application.Common.Enums;

namespace Restall.Application.Common;

/// <summary>
/// A custom Result type that can be used when you do not need to return a value.
/// Auto-sets IsSuccess bool on Success or Error.
/// Use this type to easily return messages, errors,
/// and exceptions to propagate them for useful logging and user-facing messages.
/// </summary>
/// <remarks>
///     <para>
/// The messages returned by this type should be useful for logging and not be used to write messages to users. Instead,
/// return an ErrorType with Result.Error and match on the ErrorType in a switch expression
/// to write manual user-friendly messages.
///     </para>
/// </remarks>
public sealed record Result(
    bool IsSuccess,
    string? Message = null,
    Exception? Exception = null,
    ErrorType ErrorType = ErrorType.None)
{
    public static Result Success() => new(true);

    /// <param name="message">An optional error message.</param>
    /// <param name="errorType">An optional error type defined in the ErrorType enum.</param>
    /// <param name="exception">An optional exception.</param>
    public static Result Error(string? message, ErrorType errorType = ErrorType.None,
        Exception? exception = null) => new(false, message, exception, errorType);
}

/// <summary>
/// A custom Result type that can be used as Result<![CDATA[<T>]]> to return a value.
/// Created through Success, Partial or Error, which set Status. IsSuccess is true for both Success and Partial,
/// since both carry a usable value. IsPartial is only true for Partial.
/// Use this type to easily return messages,
/// errors, warning, and exceptions to propagate them for useful logging and user-facing messages.
/// </summary>
/// <remarks>
///     <para>
/// The messages returned by this type should be useful for logging and not be used to write messages to users. Instead,
/// return an ErrorType with Result.Error or a WarningType with Result.Partial and match on it in a switch expression
/// to write manual user-friendly messages.
///     </para>
/// </remarks>
public sealed record Result<T>(
    ResultStatus Status,
    T? Value = default,
    string? Message = null,
    Exception? Exception = null,
    ErrorType ErrorType = ErrorType.None,
    WarningType WarningType = WarningType.None)
{
    public bool IsSuccess => Status is not ResultStatus.Error;
    public bool IsPartial => Status is ResultStatus.Partial;

    /// <param name="value">T value to return with the success status.</param>
    public static Result<T> Success(T value) => new(ResultStatus.Success, value);

    /// <param name="value">T value to return with the partial status.</param>
    /// <param name="message">An optional warning message.</param>
    /// <param name="warningType">An optional warning type defined in the WarningType enum.</param>
    public static Result<T> Partial(T value, string? message = null,
        WarningType warningType = WarningType.None) =>
        new(ResultStatus.Partial, value, message, WarningType: warningType);

    /// <param name="message">An optional error message.</param>
    /// <param name="errorType">An optional error type defined in the ErrorType enum.</param>
    /// <param name="exception">An optional exception.</param>
    public static Result<T> Error(string? message = null, ErrorType errorType = ErrorType.None,
        Exception? exception = null) =>
        new(ResultStatus.Error, Message: message, Exception: exception, ErrorType: errorType);
}

public enum ResultStatus { Success, Partial, Error }
