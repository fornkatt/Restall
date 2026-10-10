// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Microsoft.Win32;
using System.Runtime.Versioning;
using System.Text.Json;
using System.Text.RegularExpressions;


namespace Restall.Infrastructure.Helpers;

internal static class GameScanHelper
{
    private const string SoftwareRegistryPath = @"SOFTWARE\";
    private const string Wow64RegistryPath = @"SOFTWARE\Wow6432Node\";

    internal static string? NormalizePath(string? path)
    => NormalizePath(path, Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

    internal static string? NormalizePath(string? path, char separator, char altSeparator)
    {
        if (string.IsNullOrEmpty(path)) return null;
        return path.Replace(altSeparator, separator).Trim().TrimEnd(separator);
    }

    internal static string? ExtractVdfValue(string vdfContent, string key)
        => Regex.Match(vdfContent, $@"""{Regex.Escape(key)}""\s+""([^""]+)""",
            RegexOptions.IgnoreCase) is { Success: true } m
            ? m.Groups[1].Value
            : null;

    internal static string ReadJsonString(JsonElement json, string key) =>
        json.TryGetProperty(key, out var value) &&
        value.ValueKind == JsonValueKind.String ? value.GetString()
                                                   ?? string.Empty : string.Empty;

    [SupportedOSPlatform("windows")]
    internal static string? ReadRegistry(string keyPath, string valueName)
    {
        try
        {
            var fullPath = keyPath.StartsWith(SoftwareRegistryPath, StringComparison.OrdinalIgnoreCase)
                ? keyPath
                : SoftwareRegistryPath + keyPath;

            using var currentUserKey = Registry.CurrentUser.OpenSubKey(fullPath);
            var value = currentUserKey?.GetValue(valueName) as string;
            if (value is not null) return value;

            using var localMachineKey = Registry.LocalMachine.OpenSubKey(fullPath);
            value = localMachineKey?.GetValue(valueName) as string;
            if (value is not null) return value;

            var wow64Path = fullPath.Replace(SoftwareRegistryPath, Wow64RegistryPath);
            using var wow64Key = Registry.LocalMachine.OpenSubKey(wow64Path);
            return wow64Key?.GetValue(valueName) as string;
        }
        catch
        {
            return null;
        }
    }

    [SupportedOSPlatform("windows")]
    internal static RegistryKey? GetOpenRegistryKey(string keyPath)
    {
        try
        {
            var fullPath = keyPath.StartsWith(SoftwareRegistryPath, StringComparison.OrdinalIgnoreCase)
                ? keyPath
                : SoftwareRegistryPath + keyPath;

            var key = Registry.LocalMachine.OpenSubKey(fullPath);
            if (key is not null) return key;

            var wow64Path = fullPath.Replace(SoftwareRegistryPath, Wow64RegistryPath);
            return Registry.LocalMachine.OpenSubKey(wow64Path);
        }
        catch
        {
            return null;
        }
    }

    [SupportedOSPlatform("windows")]
    internal static string? GetRegistryValue(RegistryKey key, params string[] valueNames)
    {
        foreach (var name in valueNames)
        {
            if (key.GetValue(name) is string value && !string.IsNullOrEmpty(value))
                return value;
        }

        return null;
    }

    //TODO: CREATE MANIFEST FOR NONGAMEEXECUTABLE, NONGAME AND GETPREFERREDEXESUBFOLDERS
    internal static bool NonGameExecutable(string exeNameWithoutExtension)
    {
        var keywords = new HashSet<string>()
        {
            "UbisoftConnectInstaller",
            "EpicOnlineServiceInstaller",
            "DirectXSetup",
            "EOSBootstrapper",
            "DXSETUP",
            "vcredist",
            "UnityCrashHandler",
            "CrashReportClient",
            "launcher",
            "helper",
            "crashpad",
            "crashreport",
            "setup",
            "install",
            "unins",
            "redist",
            "DedicatedServer"
        };

        return keywords.Any(k => exeNameWithoutExtension.Contains(k, StringComparison.OrdinalIgnoreCase));
    }

    internal static bool NonGame(string name)
    {
        var nonGameArray = new HashSet<string>
        {
            "Proton",
            "Steam Linux Runtime",
            "Steamworks Common Redistributables",
            "Exodus SDK",
            "DotNET",
            "__Installer",
            "_CommonRedist",
            "UE_",
            "Lossless Scaling",
            "SteamVR"
        };
        if (nonGameArray.Any(k => name.Contains(k, StringComparison.OrdinalIgnoreCase)))
            return true;

        var nonGameSuffixes = new HashSet<string>
        {
            "Demo",
            "demo",
            "Beta",
            "beta",
            "Playtest",
            "playtest",
            "Dedicated Server"
        };
        return nonGameSuffixes.Any(s => name.EndsWith(s, StringComparison.OrdinalIgnoreCase));
    }

    internal static string[] GetPreferredExeSubFolders() =>
    [
        "bin",
        Path.Combine("bin", "x64_dx12"),
        Path.Combine("bin", "x64"),
        Path.Combine("bin", "x86"),
        Path.Combine("bin", "win64")
    ];
}
