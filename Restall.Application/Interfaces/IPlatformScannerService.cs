using System.Text.RegularExpressions;
using Restall.Application.DTOs;
using Restall.Domain.Entities;

namespace Restall.Application.Interfaces;

public interface IPlatformScannerService
{
    Task<GameScanResultDto> ScanAsync();
    
    Game.Platform Platform { get; }
    
}