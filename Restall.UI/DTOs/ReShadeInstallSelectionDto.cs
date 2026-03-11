using Restall.Domain.Entities;

namespace Restall.UI.DTOs;

public record ReShadeInstallSelectionDto(
    string Version,
    ReShade.FileName FileName,
    ReShade.FileExtension FileExtension
    );