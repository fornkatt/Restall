namespace Restall.Application.DTOs;

public record RenoDXGenericModInfoDto(
    string Name,
    string Status,
    Engine Engine,
    string? Notes = null
)

{
    public string AddonFileName64 => GetAddonFileName("64");
    public string AddonFileName32 => GetAddonFileName("32");

    private string GetAddonFileName(string bit) =>
        Engine switch
        {
            Engine.Unreal => $"renodx-unrealengine.addon{bit}",
            Engine.Unity => $"renodx-unityengine.addon{bit}",
            _ => throw new ArgumentOutOfRangeException(nameof(Engine))
        };
}

public enum Engine { Unreal, Unity }