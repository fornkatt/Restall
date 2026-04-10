using Restall.Application.DTOs;
using Restall.Domain.Entities;

namespace Restall.Application.Interfaces.Driven;

public interface IPlatformScannerService
{
    Task<GameScanResultDto> ScanAsync();
    
    Game.Platform Platform { get; }
    
}