using System.Text.RegularExpressions;

namespace Restall.Application.Helpers;

public static partial class GameNameHelper
{
    [GeneratedRegex(@"[^\w\s]")]
    private static partial Regex NonWordCharsRegex();

    public static string NormalizeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return string.Empty;

        return NonWordCharsRegex().Replace(name, string.Empty)
                                  .Replace("  ", " ")
                                  .Trim()
                                  .ToLowerInvariant();
    }
}