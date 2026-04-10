using Microsoft.Win32;
using System.Text.RegularExpressions;

namespace Restall.Infrastructure.Helpers;

internal static class GameScanHelper
{
    private const string s_softwareRegistryPath = @"SOFTWARE\";
    private const string s_wow64RegistryPath = @"SOFTWARE\Wow6432Node\";

    internal static string? NormalizePath(string? path)
    {
        if(string.IsNullOrEmpty(path)) return null;
        var normalized = path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
        return normalized.Trim().TrimEnd(Path.DirectorySeparatorChar);
    }
    
    // Dynamic inject at runtime for VdfValue and the result can vary compared to RegexHelper that is fixed at compile time with GeneredRegex
    internal static string? ExtractVdfValue(string vdfContent, string key)
        => Regex.Match(vdfContent, $@"""{Regex.Escape(key)}""\s+""([^""]+)""", 
            RegexOptions.IgnoreCase) is { Success: true } m ? m.Groups[1].Value : null;
    //Same thing as VdfValue, but it also handles escaped characters and normalises the path
    internal static string? ExtractJsonString(string json, string key) =>
        Regex.Match(json, $@"""{Regex.Escape(key)}""\s*:\s*""([^""\\]*(\\.[^""\\]*)*)""")
            is { Success: true } m ? NormalizePath(m.Groups[1].Value.Replace("\\\\", "\\").Replace("\\/", "/")) : null;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "Already checked at call site")]
    internal static string? ReadRegistry(string keyPath, string valueName)
    {
        try
        {
            var fullPath = keyPath.StartsWith(s_softwareRegistryPath, StringComparison.OrdinalIgnoreCase) ? keyPath : s_softwareRegistryPath + keyPath;

            using var currentUserKey =  Registry.CurrentUser.OpenSubKey(fullPath);
            var value =  currentUserKey?.GetValue(valueName) as string;
            if (value != null) return value;
            
            using var localMachineKey = Registry.LocalMachine.OpenSubKey(fullPath);
            value  = localMachineKey?.GetValue(valueName) as string;
            if(value != null) return value;
            
            var wow64Path = fullPath.Replace(s_softwareRegistryPath, s_wow64RegistryPath);
            using var wow64Key = Registry.LocalMachine.OpenSubKey(wow64Path);
            return wow64Key?.GetValue(valueName) as string;
            
        }
        catch { return null; }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "Already checked at call site")]
    internal static RegistryKey? GetOpenRegistryKey(string keyPath)
    {
        try
        {
            var fullPath = keyPath.StartsWith(s_softwareRegistryPath, StringComparison.OrdinalIgnoreCase)
                ? keyPath
                : s_softwareRegistryPath + keyPath;
            
            var key = Registry.LocalMachine.OpenSubKey(fullPath);
            if (key != null) return key;
            
            var wow64path = fullPath.Replace(s_softwareRegistryPath, s_wow64RegistryPath);
            return Registry.LocalMachine.OpenSubKey(wow64path);
           
        }
        catch { return null; }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "Already checked at call site")]
    internal static string? GetRegistryValue(RegistryKey key, params string[] valueNames)
    {
        foreach (var name in valueNames)
        {
            if (key.GetValue(name) is string value && !string.IsNullOrEmpty(value))
                return value;
        }
        return null;
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
            "UE_"
            
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
        Path.Combine("bin", "x64"),
        Path.Combine("bin", "x86"),
        Path.Combine("bin", "win64")
    ];



}