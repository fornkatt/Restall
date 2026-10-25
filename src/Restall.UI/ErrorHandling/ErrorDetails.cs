// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Restall.UI.ErrorHandling;

internal static class ErrorDetails
{
    internal static string Build(Exception exception, string logFolderPath, DateTimeOffset occurredAt) =>
        $"""
         Restall {GetAppVersion()}
         OS: {RuntimeInformation.OSDescription} ({RuntimeInformation.OSArchitecture})
         .NET: {RuntimeInformation.FrameworkDescription}
         Time (UTC): {occurredAt.UtcDateTime:yyyy-MM-dd HH:mm:ss}
         Log folder: {logFolderPath}

         {exception}
         """;

    private static string GetAppVersion() =>
        Assembly.GetEntryAssembly()?
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion ?? "Unknown version";
}
