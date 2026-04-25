using Restall.Application.Common;
using Restall.Application.Interfaces.Driven;
using Restall.Infrastructure.Helpers;

namespace Restall.Infrastructure.Services;

internal sealed class FileService : IFileService
{
    public Result TryDeleteFile(string path, string? verifyOriginalFilename = null)
    {
        if (!File.Exists(path))
        {
            return Result.Error($"File not found at expected location: {path}", ErrorType.FileNotFound);
        }

        try
        {
            if (verifyOriginalFilename is not null)
            {
                var originalFilename = PeVersionHelper.GetVersionInfo(path).Value?.OriginalFilename;

                if (originalFilename?.StartsWith(verifyOriginalFilename, StringComparison.OrdinalIgnoreCase) != true)
                {
                    return Result.Error(
                        $"File at {path} did not match expected prefix '{verifyOriginalFilename}' (was: '{originalFilename}'). Skipping deletion.");
                }
            }

            File.Delete(path);
            return Result.Success();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Result.Error($"Access denied trying to delete {path}. Please ensure the game is not running and try again.",
                ErrorType.PermissionDenied, ex);
        }
        catch (IOException ex)
        {
            return Result.Error($"File locked at {path}. Could not delete.", ErrorType.FileSystemError, ex);
        }
        catch (Exception ex)
        {
            return Result.Error($"Unexpected error occured. Failed to delete file at {path}", ErrorType.Unknown, ex);
        }
    }
}