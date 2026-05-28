using Restall.Domain.Entities;

namespace Restall.Application.DTOs.Results;

public record GameScanResultDto(
    Game.Platform Platform,
    IReadOnlyList<Game> Games,
    bool IsSuccess,
    string? Message = null
);