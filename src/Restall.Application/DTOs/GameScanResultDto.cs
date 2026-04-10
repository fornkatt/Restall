using Restall.Domain.Entities;

namespace Restall.Application.DTOs;

public record GameScanResultDto(
    Game.Platform Platform,
    IReadOnlyList<Game> Games,
    bool IsSuccess,
    string? Message = null
    );