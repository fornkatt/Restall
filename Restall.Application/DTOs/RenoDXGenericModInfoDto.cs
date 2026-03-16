namespace Restall.Application.DTOs;

public record RenoDXGenericModInfoDto(
    string Name,
    string Status,
    Engine Engine,
    string? Notes = null
)

{
    public string AddonFilename64 => GetAddonFilename("64");
    public string AddonFilename32 => GetAddonFilename("32");

    private string GetAddonFilename(string bit) =>
        Engine switch
        {
            Engine.Unreal => $"renodx-unrealengine.addon{bit}",
            Engine.Unity => $"renodx-unityengine.addon{bit}",
            _ => throw new ArgumentOutOfRangeException(nameof(Engine))
        };
}

public enum Engine { Unreal, Unity }