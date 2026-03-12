using System.Text.RegularExpressions;
using Restall.Domain.Entities;

namespace Restall.Application.Interfaces;

public interface IPlatformScannerService
{
    Task<List<Game>> ScanAsync();
    
}