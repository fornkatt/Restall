using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace Restall.Infrastructure.Helpers;

public class Helper
{

    public static string? NormalizePath(string path)
    {
        if(string.IsNullOrEmpty(path)) return null;
        var normalized = path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
        return normalized.Trim().TrimEnd(Path.DirectorySeparatorChar);
    }

    public static string? ExtractVdfValue(string vdfContent, string key)
        => Regex.Match(vdfContent, $@"""{Regex.Escape(key)}""\s+""([^""]+)""", 
            RegexOptions.IgnoreCase) is { Success: true } m ? m.Groups[1].Value : null;
    
    public static string? ExtractJsonString(string json, string key) =>
        Regex.Match(json, $@"""{Regex.Escape(key)}""\s*:\s*""([^""\\]*(\\.[^""\\]*)*)""")
            is { Success: true } m ? NormalizePath(m.Groups[1].Value.Replace("\\\\", "\\").Replace("\\/", "/")) : null;
    
    public static string? ReadRegistry(string keyPath, string valueName)
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(keyPath)
                            ?? Registry.LocalMachine.OpenSubKey(keyPath);
            return key?.GetValue(valueName) as string;
        }
        catch { return null; }
    }

    public static bool NonGame(string name)
    {
        var nonGameArray = new[]
        {
            "Proton",
            "Steam Linux Runtime",
            "Steamworks Common Redistributables",
            "Exodus SDK",
            "DotNET",
            "_Installer",
            "_CommonRedist"
            
        };
        return nonGameArray.Any(k => name.Contains(k, StringComparison.OrdinalIgnoreCase));
    }
    
    
    
    
}