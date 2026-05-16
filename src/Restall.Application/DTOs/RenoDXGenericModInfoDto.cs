namespace Restall.Application.DTOs;

public record RenoDXGenericModInfoDto(
    string Name,
    string Status,
    SupportedEngine Engine,
    Architecture Architecture = Architecture.x64,
    string? Notes = null
)

{
    public string AddonFilename64 => GetAddonFilename("64");
    public string AddonFilename32 => GetAddonFilename("32");
    
    public bool SupportsX64 => !SupportsX32;
    public bool SupportsX32 => Architecture == Architecture.x32;

    private string GetAddonFilename(string bit) =>
        Engine switch
        {
            SupportedEngine.Unreal => $"renodx-unrealengine.addon{bit}",
            SupportedEngine.Unity => $"renodx-unityengine.addon{bit}",
            _ => "unknown"
        };
}

public enum SupportedEngine { Unreal, Unity }

public enum Architecture { x32 = 32, x64 = 64 }