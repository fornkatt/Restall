using Restall.Domain.Entities;

namespace Restall.UI.DTOs;

public record ReShadeInstallSelectionDto(
    string Version,
    ReShade.Filename Filename,
    ReShade.FileExtension FileExtension
    );