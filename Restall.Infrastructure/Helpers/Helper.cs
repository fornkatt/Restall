using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace Restall.Infrastructure.Helpers;

internal class Helper
{
    private const string SoftwareRegistryPath = @"SOFTWARE\";
    private const string Wow64RegistryPath = @"SOFTWARE\Wow6432Node\";

    internal static string? NormalizePath(string path)
    {
        if(string.IsNullOrEmpty(path)) return null;
        var normalized = path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
        return normalized.Trim().TrimEnd(Path.DirectorySeparatorChar);
    }

    internal static string? ExtractVdfValue(string vdfContent, string key)
        => Regex.Match(vdfContent, $@"""{Regex.Escape(key)}""\s+""([^""]+)""", 
            RegexOptions.IgnoreCase) is { Success: true } m ? m.Groups[1].Value : null;
    
    internal static string? ExtractJsonString(string json, string key) =>
        Regex.Match(json, $@"""{Regex.Escape(key)}""\s*:\s*""([^""\\]*(\\.[^""\\]*)*)""")
            is { Success: true } m ? NormalizePath(m.Groups[1].Value.Replace("\\\\", "\\").Replace("\\/", "/")) : null;
    
    internal static string? ReadRegistry(string keyPath, string valueName)
    {
        try
        {
            var fullPath = keyPath.StartsWith(SoftwareRegistryPath, StringComparison.OrdinalIgnoreCase) ? keyPath : SoftwareRegistryPath + keyPath;
            
            using var currentUserKey =  Registry.CurrentUser.OpenSubKey(fullPath);
            var value =  currentUserKey?.GetValue(valueName) as string;
            if(value != null) return value;
            
            using var localMachineKey = Registry.LocalMachine.OpenSubKey(fullPath);
            value  = localMachineKey?.GetValue(valueName) as string;
            if(value != null) return value;
            
            var wow64Path = fullPath.Replace(SoftwareRegistryPath, Wow64RegistryPath);
            using var wow64Key = Registry.LocalMachine.OpenSubKey(wow64Path);
            return wow64Key?.GetValue(valueName) as string;
            
        }
        catch { return null; }
    }

    internal static RegistryKey? GetOpenRegistryKey(string keyPath)
    {
        try
        {
            var fullPath = keyPath.StartsWith(SoftwareRegistryPath, StringComparison.OrdinalIgnoreCase)
                ? keyPath
                : SoftwareRegistryPath + keyPath;
            
            var key = Registry.LocalMachine.OpenSubKey(fullPath);
            if (key != null) return key;
            
            var wow64path = fullPath.Replace(SoftwareRegistryPath, Wow64RegistryPath);
            return Registry.LocalMachine.OpenSubKey(wow64path);
           
        }
        catch { return null; }
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
            "_CommonRedist"
            
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
    
}