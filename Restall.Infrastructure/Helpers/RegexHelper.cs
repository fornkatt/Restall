using System.Text.RegularExpressions;

namespace Restall.Infrastructure.Helpers;

internal static partial class RegexHelper
{
    internal static Regex RenoDXVersionRegex => RenoDXVersion();
    internal static Regex ExtractReShadeVersionFromSite => ExtractReShadeFromSite();
    internal static Regex SteamLibraryRegex => SteamLibrary();
    internal static Regex HeroicGameBlockRegex => HeroicGameBlock();
    internal static Regex HeroicInstallPathRegex => HeroicInstallPath();
    internal static Regex HeroicTitleRegex => HeroicTitle();
    
    [GeneratedRegex(@"^\d+\.(\d{4})\.(\d{4})\.\d+$")]
    private static partial Regex RenoDXVersion();

    [GeneratedRegex(@"ReShade (\d+\.\d+\.\d+)")]
    private static partial Regex ExtractReShadeFromSite();
    
    [GeneratedRegex(@"""path""\s+""([^""]+)""")]
    private static partial Regex SteamLibrary();
    
    [GeneratedRegex(@"\{[^{}]*""install_path""[^{}]*\}")]
    private static partial Regex HeroicGameBlock();
    
    [GeneratedRegex(@"""install_path""\s*:\s*""([^""]+)""")]
    private static partial Regex HeroicInstallPath();
    
    [GeneratedRegex(@"""title""\s*:\s*""([^""]+)""")]
    private static partial Regex HeroicTitle();



}