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
    internal static Regex GOGHeroicAppNameRegex => HeroicAppNameGOG();
    internal static Regex EpicHeroicAppNameRegex => HeroicAppNameEpic();
    internal static Regex Match32BitRegex => Match32Bit();

    [GeneratedRegex(@"\b32[\s-]?bit\b", RegexOptions.IgnoreCase)]
    private static partial Regex Match32Bit();
    
    [GeneratedRegex(@"^\d+\.(\d{4})\.(\d{4})\.\d+$")]
    private static partial Regex RenoDXVersion();

    [GeneratedRegex(@"ReShade (\d+\.\d+\.\d+)")]
    private static partial Regex ExtractReShadeFromSite();
    
    [GeneratedRegex(@"""path""\s+""([^""]+)""")]
    private static partial Regex SteamLibrary();

    [GeneratedRegex(@"""appName""\s*:\s*""([^""]+)""")]
    private static partial Regex HeroicAppNameGOG();
    
    [GeneratedRegex(@"""app_name""\s*:\s*""([^""]+)""")]
    private static partial Regex HeroicAppNameEpic();
    
    [GeneratedRegex(@"\{[^{}]*""install_path""[^{}]*\}")]
    private static partial Regex HeroicGameBlock();
    
    [GeneratedRegex(@"""install_path""\s*:\s*""([^""]+)""")]
    private static partial Regex HeroicInstallPath();
    
    [GeneratedRegex(@"""title""\s*:\s*""([^""]+)""")]
    private static partial Regex HeroicTitle();
}