using System.Threading.Tasks;

namespace Restall.Services;

public interface IFileExtractionService
{
    Task<bool> ExtractFiles(string? targetPath = null, string[]? targetFiles = null, string? destinationPath = null);
}