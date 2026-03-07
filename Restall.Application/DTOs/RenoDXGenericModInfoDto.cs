namespace Restall.Application.DTOs;

public record RenoDXGenericModInfoDto(
    string Name,
    string Status,
    string? Notes,
    Engine Engine
    );

public enum Engine { Unreal, Unity }