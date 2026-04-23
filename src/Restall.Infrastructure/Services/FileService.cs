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
            return Result.Err($"File not found at expected location: {path}", ResultError.FileNotFound);
        }

        try
        {
            if (verifyOriginalFilename is not null)
            {
                var originalFilename = PeVersionHelper.GetVersionInfo(path).Value?.OriginalFilename;

                if (originalFilename?.StartsWith(verifyOriginalFilename, StringComparison.OrdinalIgnoreCase) != true)
                {
                    return Result.Err(
                        $"File at {path} did not match expected prefix '{verifyOriginalFilename}' (was: '{originalFilename}'). Skipping deletion.");
                }
            }

            File.Delete(path);
            return Result.Ok();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Result.Err($"Access denied trying to delete {path}. Please ensure the game is not running and try again.",
                ResultError.PermissionDenied, ex);
        }
        catch (IOException ex)
        {
            return Result.Err($"File locked at {path}. Could not delete.", ResultError.FileSystemError, ex);
        }
        catch (Exception ex)
        {
            return Result.Err($"Unexpected error occured. Failed to delete file at {path}", ResultError.Unknown, ex);
        }
    }
}