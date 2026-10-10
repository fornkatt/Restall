// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Microsoft.Extensions.Logging;
using Restall.Domain.Entities;

namespace Restall.Application.UseCases;

// RenoDX Install Logging — EventId range: 1300 - 1349
public sealed partial class InstallRenoDXUseCase
{
    [LoggerMessage(EventId = 1300, Level = LogLevel.Error,
        Message = "Could not find a RenoDX download for \"{GameName}\" on the {Branch} branch")]
    private partial void LogRenoDXDownloadNotFound(string gameName, RenoDX.Branch branch);

    [LoggerMessage(EventId = 1301, Level = LogLevel.Warning,
        Message =
            "Failed to read version from RenoDX file \"{Filename}\" for \"{GameName}\"" +
            " — Service returned: \"{ErrorMessage}\"")]
    private partial void LogRenoDXVersionReadFailure(string filename, string gameName, string? errorMessage,
        Exception? ex);
}
