// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Microsoft.Extensions.Logging;

namespace Restall.Application.UseCases;

// Open Web Link Logging — EventId range: 2150 - 2199
public sealed partial class OpenWebLinkUseCase
{
    [LoggerMessage(EventId = 2150, Level = LogLevel.Warning,
        Message = "Refused to open \"{Url}\" because it isn't a plain https link")]
    private partial void LogLinkRefused(string url);

    [LoggerMessage(EventId = 2151, Level = LogLevel.Error,
        Message = "Failed to open \"{Url}\" — Service returned: \"{ErrorMessage}\"")]
    private partial void LogWebLinkOpenFailure(string url, string? errorMessage, Exception? ex);
}
