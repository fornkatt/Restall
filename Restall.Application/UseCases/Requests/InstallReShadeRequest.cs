using Restall.Domain.Entities;

namespace Restall.Application.UseCases.Requests;

public record InstallReShadeRequest(
    Game Game,
    ReShade.Branch Branch,
    ReShade.Architecture Arch,
    string Version,
    string SelectedFilename
    );