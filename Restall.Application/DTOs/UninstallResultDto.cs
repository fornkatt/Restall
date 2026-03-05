using Restall.Domain.Entities;

namespace Restall.Application.DTOs;

public class UninstallResultDto
{
    public Game UpdatedGame { get; set; } = null!;
    public bool ShouldPromptForDeepScan { get; set; }
}