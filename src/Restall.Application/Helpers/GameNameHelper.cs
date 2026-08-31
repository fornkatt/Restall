using System.Text.RegularExpressions;

namespace Restall.Application.Helpers;

public static partial class GameNameHelper
{
    private static readonly Dictionary<char, int> RomanValues = new()
    {
        ['i'] = 1, ['v'] = 5, ['x'] = 10, ['l'] = 50,
        ['c'] = 100, ['d'] = 500, ['m'] = 1000
    };

    [GeneratedRegex(@"[^\w\s]")]
    private static partial Regex NonWordCharsRegex();

    [GeneratedRegex(
        @"(\s*[-–:]\s*|\s+)(ultimate|complete|gold|deluxe|premium|anniversary|directors cut|goty|game of the year|standard)(\s+edition)?\s*$",
        RegexOptions.IgnoreCase)]
    private static partial Regex EditionSuffixRegex();

    [GeneratedRegex(@"(\s*[-–:]\s*|\s+)(remastered|remake|definitive|enhanced|enchanted)(\s+edition)?\s*$",
        RegexOptions.IgnoreCase)]
    private static partial Regex DistinctEditionSuffixRegex();

    [GeneratedRegex(@"^.*[-–]\s+(?<subtitle>\S.+)$")]
    private static partial Regex SubtitleSeparatorRegex();

    [GeneratedRegex(@"[\u2018\u2019\u02BC\u0060\u00B4]")]
    private static partial Regex ApostropheVariantRegex();

    [GeneratedRegex(
        @"^M{0,4}(CM|CD|D?C{0,3})(XC|XL|L?X{0,3})(IX|IV|V?I{0,3})$",
        RegexOptions.IgnoreCase)]
    private static partial Regex RomanNumeralRegex();

    [GeneratedRegex(@"\s*\(.*?\)\s*")]
    private static partial Regex ParentheticalRegex();

    [GeneratedRegex(@"'s\b", RegexOptions.IgnoreCase)]
    private static partial Regex PossessiveRegex();

    public static string NormalizeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return string.Empty;

        var canonicalized = ApostropheVariantRegex().Replace(name, "'");
        var withoutEdition = StripEditionSuffix(canonicalized);
        var withoutParentheticals = ParentheticalRegex().Replace(withoutEdition, " ").Trim();
        var withoutPossessiveness = PossessiveRegex().Replace(withoutParentheticals, string.Empty)
            .Trim();

        var cleanedName = NonWordCharsRegex().Replace(withoutPossessiveness, string.Empty)
            .Replace("  ", " ")
            .Trim()
            .ToLowerInvariant();

        return NormalizeNumerals(cleanedName);
    }

    public static string StripEditionSuffix(string editionSuffix) =>
        EditionSuffixRegex().Replace(editionSuffix,
                string.Empty)
            .Trim();

    public static string StripCollectionPartSuffix(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return name;

        var collection = GameCollectionCatalog.All.FirstOrDefault(d =>
            d.CollapseForModMatching && d.Matches(name));

        if (collection is null)
            return name;

        foreach (var part in collection.PartSuffixes)
        {
            var pattern = $@"\s*[-–]\s*{Regex.Escape(part)}$";

            if (Regex.IsMatch(name, pattern, RegexOptions.IgnoreCase))
                return Regex.Replace(name, pattern, string.Empty, RegexOptions.IgnoreCase).TrimEnd();
        }

        return name;
    }

    public static bool FuzzyNameMatch(string a, string b)
    {
        var aWords = a.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var bWords = b.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var aSet = new HashSet<string>(aWords);
        var bSet = new HashSet<string>(bWords);

        var aHasNumeral = aWords.Any(w => w.All(char.IsDigit));
        var bHasNumeral = bWords.Any(w => w.All(char.IsDigit));
        if (aHasNumeral != bHasNumeral)
            return false;

        var aContainsAllBWords = bWords.All(aSet.Contains);
        var bContainsAllAWords = aWords.All(bSet.Contains);

        if (!aContainsAllBWords && !bContainsAllAWords)
            return false;

        var shared = aWords.Count(bSet.Contains);
        var maxWords = Math.Max(aWords.Length, bWords.Length);

        if (maxWords > 0 && (double)shared / maxWords >= 0.5)
            return true;

        var (shorterDistinct, longerSet) = aWords.Length <= bWords.Length
            ? (aWords.Distinct().ToArray(), new HashSet<string>(bWords))
            : (bWords.Distinct().ToArray(), new HashSet<string>(aWords));

        return shorterDistinct.Length >= 2 && shorterDistinct.All(w => longerSet.Contains(w));
    }

    public static bool IsLikelySameGame(string? a, string? b)
    {
        if (string.IsNullOrWhiteSpace(a) || string.IsNullOrWhiteSpace(b))
            return false;

        var normA = NormalizeName(a);
        var normB = NormalizeName(b);

        if (normA == normB)
            return true;

        if (!FuzzyNameMatch(normA, normB))
            return false;

        var editionA = ExtractDistinctEdition(a);
        var editionB = ExtractDistinctEdition(b);

        if (editionA is not null && editionB is not null &&
            !string.Equals(editionA, editionB, StringComparison.OrdinalIgnoreCase))
            return false;

        return !HasUnrelatedTrailingSubtitle(a, normB) &&
               !HasUnrelatedTrailingSubtitle(b, normA);
    }

    private static string? ExtractDistinctEdition(string name) =>
        DistinctEditionSuffixRegex()
                .Match(StripEditionSuffix(ApostropheVariantRegex().Replace(name, "'")))
            is { Success: true } m
            ? m.Groups[2].Value.ToLowerInvariant()
            : null;

    private static bool HasUnrelatedTrailingSubtitle(string raw, string otherNormalized)
    {
        var stripped = StripEditionSuffix(ApostropheVariantRegex().Replace(raw, "'"));

        if (SubtitleSeparatorRegex().Match(stripped) is not { Success: true } match)
            return false;

        var subtitleWords = NormalizeName(match.Groups["subtitle"].Value)
            .Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (subtitleWords.Length == 0)
            return false;

        var otherWords = new HashSet<string>(otherNormalized.Split(' ', StringSplitOptions.RemoveEmptyEntries));

        return subtitleWords.All(w => !otherWords.Contains(w));
    }

    private static string NormalizeNumerals(string name)
    {
        var words = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        return string.Join(' ', words.Select((w, index) =>
        {
            if (!RomanNumeralRegex().IsMatch(w))
                return w;

            if (w.Length >= 2)
                return ParseRoman(w).ToString();

            var isLast = index == words.Length - 1;
            var nextIsVariantCode = !isLast
                                    && words[index + 1].Length == 1
                                    && char.IsLetter(words[index + 1][0]);
            var prevLooksLikeSeries = index >= 1
                                      && words[index - 1].Length > 1;

            return isLast || nextIsVariantCode || prevLooksLikeSeries
                ? ParseRoman(w).ToString()
                : w;
        }));
    }

    private static int ParseRoman(string roman)
    {
        var result = 0;

        for (var i = 0; i < roman.Length; i++)
        {
            var current = RomanValues[roman[i]];
            var next = i + 1 < roman.Length ? RomanValues[roman[i + 1]] : 0;
            result += current < next ? -current : current;
        }

        return result;
    }
}