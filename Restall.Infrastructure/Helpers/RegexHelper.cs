using System.Text.RegularExpressions;

namespace Restall.Infrastructure.Helpers;

internal static partial class RegexHelper
{
    internal static Regex RenoDXVersionRegex => RenoDXVersion();
    internal static Regex ExtractReShadeVersionFromSite => ExtractReShadeFromSite();

    [GeneratedRegex(@"^\d+\.(\d{4})\.(\d{4})\.\d+$")]
    private static partial Regex RenoDXVersion();

    [GeneratedRegex(@"ReShade (\d+\.\d+\.\d+)")]
    private static partial Regex ExtractReShadeFromSite();
}