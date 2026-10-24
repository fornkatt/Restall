// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Application.Common;
using Restall.Application.Common.Enums;
using Restall.Application.Interfaces.Driven;
using System.ComponentModel;
using System.Diagnostics;

namespace Restall.Infrastructure.Services;

internal sealed class WebLinkLauncher : IWebLinkLauncher
{
    public Result OpenWebLink(Uri url)
    {
        if (!IsWebLink(url))
            throw new ArgumentException("Only absolute http or https links can be opened as web links.", nameof(url));

        try
        {
            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = url.AbsoluteUri,
                UseShellExecute = true
            });

            return Result.Success();
        }
        catch (Win32Exception ex)
        {
            return Result.Error("Failed to start the default app for the link", ErrorType.ProcessStartFailed,
                ex);
        }
        catch (PlatformNotSupportedException ex)
        {
            return Result.Error("Opening links isn't supported on this platform", ErrorType.ProcessStartFailed,
                ex);
        }
    }

    private static bool IsWebLink(Uri url) =>
        url is { IsAbsoluteUri: true, Scheme: "http" or "https" };
}
