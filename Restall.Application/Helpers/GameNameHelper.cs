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
    
    [GeneratedRegex(
        @"(\s*[-–:]\s*|\s+)(ultimate|complete|definitive|gold|deluxe|premium|anniversary|enhanced|enchanted|directors cut|remastered|goty|game of the year|standard)(\s+edition)?\s*$"
        ,
        RegexOptions.IgnoreCase)]
    private static partial Regex EditionSuffixRegex();
    

    public static string StripEditionSuffix(string editionSuffix) =>
        EditionSuffixRegex().Replace(editionSuffix, 
                string.Empty)
            .Trim();

    public static bool FuzzyNameMatch(string a, string b)
    {
        if (!a.Contains(b) && !b.Contains(a))
            return false;

        var aWords = a.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var bWords = b.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var bSet = new HashSet<string>(bWords);

        int shared = aWords.Count(w => bSet.Contains(w));
        int maxWords = Math.Max(aWords.Length, bWords.Length);

        return maxWords > 0 && (double)shared / maxWords >= 0.5;
    }

    public static double ComputeNameSimilarity(string a, string b)
    {
        if (!a.Contains(b) && !b.Contains(a)) return 0.0;

        var aWords = a.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var bWords = b.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var bSet = new HashSet<string>(bWords);

        int shared = aWords.Count(w => bSet.Contains(w));
        int total = aWords.Length + bWords.Length;

        return total == 0 ? 0.0 : (2.0 * shared) / total;

    }
    
    
}