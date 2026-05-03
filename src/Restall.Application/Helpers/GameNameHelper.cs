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
        @"(\s*[-–:]\s*|\s+)(ultimate|complete|definitive|gold|deluxe|premium|anniversary|enhanced|enchanted|directors cut|remastered|remake|goty|game of the year|standard)(\s+edition)?\s*$"
        ,
        RegexOptions.IgnoreCase)]
    private static partial Regex EditionSuffixRegex();


    public static string StripEditionSuffix(string editionSuffix) =>
        EditionSuffixRegex().Replace(editionSuffix,
                string.Empty)
            .Trim();

    public static bool FuzzyNameMatch(string a, string b)
    {
        var aWords = a.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var bWords = b.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var aSet = new HashSet<string>(aWords);
        var bSet = new HashSet<string>(bWords);

        var aContainsAllBWords = bWords.All(w => aSet.Contains(w));
        var bContainsAllAWords = aWords.All(w => bSet.Contains(w));

        if (!aContainsAllBWords && !bContainsAllAWords)
            return false;

        var shared = aWords.Count(w => bSet.Contains(w));
        var maxWords = Math.Max(aWords.Length, bWords.Length);

        if (maxWords > 0 && (double)shared / maxWords >= 0.5)
            return true;

        var (shorterDistinct, longerSet) = aWords.Length <= bWords.Length
            ? (aWords.Distinct().ToArray(), new HashSet<string>(bWords))
            : (bWords.Distinct().ToArray(), new HashSet<string>(aWords));

        return shorterDistinct.Length >= 2 && shorterDistinct.All(w => longerSet.Contains(w));
    }
}