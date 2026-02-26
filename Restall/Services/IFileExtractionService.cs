namespace Restall.Services;

public interface IFileExtractionService
{
    string? ExtractFiles(string? targetPath = null, string[]? targetFiles = null, string? destinationPath = null);
}