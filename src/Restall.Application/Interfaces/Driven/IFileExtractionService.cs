namespace Restall.Application.Interfaces.Driven;

public interface IFileExtractionService
{
    bool ExtractFiles(string fileToOpen, string[] filesToExtract, string destinationPath);
}