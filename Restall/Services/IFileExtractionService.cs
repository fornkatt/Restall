namespace Restall.Services;

public interface IFileExtractionService
{
    bool ExtractFiles(string? targetPath = null, string[]? targetFiles = null, string? destinationPath = null);
}