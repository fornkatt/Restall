using Microsoft.Extensions.Logging;
using Restall.Application.Helpers;
using Restall.Application.Interfaces.Driven;
using Restall.Application.Logging;
using Restall.Infrastructure.Helpers;

namespace Restall.Infrastructure.Services;

// TODO: surface Result/Result<T> in applicable methods. Use ErrorType, log at call-site if appropriate
// TODO: INCLUDE LOCAL ICONS AND USE PEICONHELPER AS FALLBACK
// TODO(logging-refactor): just swap the logging implementations
internal sealed partial class GameIconService : IGameIconService
{
    private readonly IIconConverterService _iconConverterService;
    private readonly ILogger<GameIconService> _logger;

    public GameIconService(
        ILogger<GameIconService> logger,
        IIconConverterService iconConverterService
    )
    {
        _logger = logger;
        _iconConverterService = iconConverterService;
    }

    public async Task ExtractIconIfMissingAsync(string? executablePath, string? gameName, string iconPath)
    {
        if (File.Exists(iconPath)) return;

        var exePath = ResolveMainExecutablePath(executablePath, gameName);
        if (exePath is null) return;

        try
        {
            var iconBytes = await Task.Run(() => PeIconHelper.ExtractLargestIconAsPng(exePath));
            if (iconBytes is null)
            {
                _logger.PeFileIconScanFailure(exePath);
                return;
            }

            if (!PeIconHelper.IsPng(iconBytes))
                iconBytes = _iconConverterService.IcoToPng(iconBytes, 256);

            await File.WriteAllBytesAsync(iconPath, iconBytes);
            LogIconExtractionSuccess(gameName ?? "Unknown", iconPath);
        }
        catch (Exception ex)
        {
            LogIconExtractionFailure(gameName ?? "Unknown", ex);
        }
    }

    private static string? ResolveMainExecutablePath(string? executableDirectory, string? gameName)
    {
        if (string.IsNullOrWhiteSpace(executableDirectory) || !Directory.Exists(executableDirectory))
            return null;

        var exes = Directory.EnumerateFiles(executableDirectory, "*.exe")
            .Where(e => !GameScanHelper.NonGameExecutable(Path.GetFileNameWithoutExtension(e)))
            .ToArray();

        if (exes.Length == 0) return null;
        if (exes.Length == 1) return exes[0];

        var normalized = GameNameHelper.NormalizeName(gameName ?? string.Empty);
        var stripped = GameNameHelper.StripEditionSuffix(normalized);

        var match = exes.FirstOrDefault(e =>
        {
            var exeName = GameNameHelper.NormalizeName(Path.GetFileNameWithoutExtension(e));
            return exeName.Contains(normalized) ||
                   exeName.Contains(stripped) ||
                   normalized.Contains(exeName) ||
                   GameNameHelper.FuzzyNameMatch(normalized, exeName) ||
                   GameNameHelper.FuzzyNameMatch(stripped, exeName);
        });

        return match ?? exes.OrderByDescending(e => new FileInfo(e).Length).First();
    }
}