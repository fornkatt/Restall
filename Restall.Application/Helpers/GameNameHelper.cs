using System.Text.RegularExpressions;

namespace Restall.Application.Helpers;

public static class GameNameHelper
{
    public static string NormalizeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return string.Empty;

        return Regex.Replace(name, @"[^\w\s]", string.Empty)
                    .Replace("  ", " ")
                    .Trim()
                    .ToLowerInvariant();
    }
}
