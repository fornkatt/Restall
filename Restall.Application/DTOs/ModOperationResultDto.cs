using Restall.Domain.Entities;

namespace Restall.Application.DTOs;

public record ModOperationResultDto(
    bool IsSuccess,
    Game UpdatedGame,
    string? Message = null,
    bool ShouldPromptForDeepScan = false,
    UpdateCheckResultDto? UpdateCheckResult = null
    );