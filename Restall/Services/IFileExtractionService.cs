namespace Restall.Services;

public interface IFileExtractionService
{
    bool ExtractFiles(string fileToOpen, string[] filesToExtract, string destinationPath);
}