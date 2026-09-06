namespace Restall.Application.DTOs;

public record RenoDXGenericModInfoDto(
    string Name,
    string Status,
    ModType ModType,
    Architecture Architecture = Architecture.x64,
    string? Notes = null
)

{
    public string AddonFilename64 => GetAddonFilename("64");
    public string AddonFilename32 => GetAddonFilename("32");

    public bool SupportsX64 => !SupportsX32;
    public bool SupportsX32 => Architecture == Architecture.x32;

    public bool IsExternallyHosted => ModType.IsExternallyHosted();

    public static string GetAddonFilename(ModType modType, string bit) =>
        modType switch
        {
            ModType.Unreal => $"renodx-unrealengine.addon{bit}",
            ModType.UnrealExtended => $"renodx-ue-extended.addon{bit}",
            ModType.Unity => $"renodx-unityengine.addon{bit}",
            _ => "unknown"
        };
    
    private string GetAddonFilename(string bit) => GetAddonFilename(ModType, bit);
}

public enum Architecture
{
    x32 = 32,
    x64 = 64
}

public enum ModType
{
    Unreal,
    UnrealExtended,
    Unity
}

public static class ModTypeExtensions
{
    public static bool IsExternallyHosted(this ModType modType) =>
        modType is ModType.UnrealExtended or ModType.Unity;
}