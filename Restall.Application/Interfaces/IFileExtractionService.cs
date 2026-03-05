namespace Restall.Application.Interfaces;

public interface IFileExtractionService
{
    bool ExtractFiles(string fileToOpen, string[] filesToExtract, string destinationPath);
}