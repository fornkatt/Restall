using Restall.Application.Helpers;
using Restall.Application.Interfaces.Driven;
using Restall.Infrastructure.Helpers;

namespace Restall.Infrastructure.Services;

internal sealed class GameIconService : IGameIconService
{
    private readonly ILogService _logService;

    public GameIconService(ILogService logService)
    {
        _logService = logService;
    }

    public async Task ExtractIconIfMissingAsync(string? executablePath, string? gameName, string iconPath)
    {
        if (File.Exists(iconPath)) return;

        var exePath = ResolveMainExecutablePath(executablePath, gameName);
        if (exePath is null) return;

        try
        {
            var iconBytes = await Task.Run(() => PeIconHelper.ExtractLargestIconAsPng(exePath));
            if (iconBytes is null) return;

            await File.WriteAllBytesAsync(iconPath, iconBytes);
            await _logService.LogInfoAsync($"Extracted icon for [{gameName}] to [{iconPath}]");
        }
        catch (Exception ex)
        {
            await _logService.LogErrorAsync($"Failed to extract icon for [{gameName}]", ex);
        }
    }

    private static string? ResolveMainExecutablePath(string? executablePath, string? gameName)
    {
        if (string.IsNullOrWhiteSpace(executablePath) || !Directory.Exists(executablePath))
            return null;

        var exes = Directory.GetFiles(executablePath, "*.exe")
            .Where(e => !GameScanHelper.NonGameExecutable(Path.GetFileNameWithoutExtension(e)))
            .ToArray();

        switch (exes.Length)
        {
            case 0: return null;
            case 1: return exes[0];
        }

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