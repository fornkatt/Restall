namespace Restall.Application.DTOs;

public record RenoDXGenericModInfoDto(
    string Name,
    string Status,
    SupportedEngine Engine,
    string? Notes = null
)

{
    public string AddonFilename64 => GetAddonFilename("64");
    public string AddonFilename32 => GetAddonFilename("32");

    private string GetAddonFilename(string bit) =>
        Engine switch
        {
            SupportedEngine.Unreal => $"renodx-unrealengine.addon{bit}",
            SupportedEngine.Unity => $"renodx-unityengine.addon{bit}",
            _ => throw new ArgumentOutOfRangeException(nameof(Engine))
        };
}

public enum SupportedEngine { Unreal, Unity }