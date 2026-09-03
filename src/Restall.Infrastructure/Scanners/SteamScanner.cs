using Restall.Application.Interfaces.Driven;
using Restall.Domain.Entities;
using Restall.Infrastructure.Helpers;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Restall.Application.DTOs.Results;

namespace Restall.Infrastructure.Scanners;

// TODO: surface Result/Result<T> in applicable methods. Use ErrorType, log at call-site if appropriate

// TODO(logging-refactor): just swap the logging implementations
internal sealed partial class SteamScanner : IPlatformScannerService
{
    private readonly ILogger<SteamScanner> _logger;

    public SteamScanner(
        ILogger<SteamScanner> logger
    )
    {
        _logger = logger;
    }

    public Task<GameScanResultDto> ScanAsync() => Task.Run(ScanSteam);
    public Game.Platform Platform => Game.Platform.Steam;

    private GameScanResultDto ScanSteam()
    {
        var games = new List<Game>();
        var errors = new List<string>();
        var steamPath = GetInstallPath();

        steamPath = GameScanHelper.NormalizePath(steamPath);

        if (string.IsNullOrWhiteSpace(steamPath))
            return new GameScanResultDto(
                Game.Platform.Steam,
                [],
                IsSuccess: false,
                Message: "Steam installation not found");


        var (steamLibraries, libraryError) = GetSteamLibraries(steamPath);
        if (libraryError is not null) errors.Add(libraryError);

        foreach (var library in steamLibraries)
        {
            var (libraryGames, error) = ScanSteamLibrary(library);
            games.AddRange(libraryGames);
            if (error is not null) errors.Add(error);
        }

        return new GameScanResultDto(
            Platform: Game.Platform.Steam,
            Games: games,
            IsSuccess: games.Count > 0,
            Message: errors.Count > 0 ? string.Join(", ", errors) : null);
    }

    private string? GetInstallPath()
    {
        if (OperatingSystem.IsWindows()) return GameScanHelper.ReadRegistry(@"Valve\Steam", "SteamPath");

        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        //TODO: DEFINE THESE IN PATHSERVICE TO HAVE A CENTRALIZED STATE SERVICE
        var linuxPaths = new[]
        {
            Path.Combine(home, ".steam", "steam"),
            Path.Combine(home, ".local", "share", "Steam"),
            Path.Combine(home, "snap", "steam", "common", ".local", "share", "Steam")
        };

        return linuxPaths.FirstOrDefault(Directory.Exists);
    }

    private (List<Game> games, string? error) ScanSteamLibrary(string library)
    {
        var games = new List<Game>();
        var steamapps = Path.Combine(library, "steamapps");

        if (!Directory.Exists(steamapps))
        {
            LogSteamAppsNotFoundInLibraryFolder(steamapps);
            return (games, null);
        }


        foreach (var acf in Directory.GetFiles(steamapps, "appmanifest_*.acf"))
        {
            var appId = Path.GetFileNameWithoutExtension(acf).Replace("appmanifest_", "");
            
            try
            {
                var content = File.ReadAllText(acf);
                var name = GameScanHelper.ExtractVdfValue(content, "name");
                var installDir = GameScanHelper.ExtractVdfValue(content, "installdir");

                if (name is null)
                {
                    LogSteamGameNameNotFound(appId,acf);
                    continue;
                }

                if (installDir is null)
                {
                    LogSteamInstallDirectoryNotFound(name,appId);
                    continue;
                }

                if (GameScanHelper.NonGame(name)) continue;
                if (GameScanHelper.NonGame(installDir)) continue;
                
                var rootPath = Path.Combine(steamapps, "common", installDir);

                if (!Directory.Exists(rootPath))
                {
                    LogSteamRootPathNotFound(rootPath,name,appId);
                    continue;
                }


                games.AddRange(GameExpander.ExpandCollection(new Game
                {
                    Name = name,
                    InstallFolder = rootPath,
                    PlatformName = Platform,
                    PlatformId = appId
                }));
            }
            catch (Exception ex)
            {
                LogSteamFailedToParseAcfFiles(acf, ex);
            }
        }

        return (games, null);
    }

    private (List<string> libraries, string? error) GetSteamLibraries(string path)
    {
        var libraries = new List<string>();
        var vdfPath = Path.Combine(path, "steamapps", "libraryfolders.vdf");
        if (!File.Exists(vdfPath)) return (libraries, null);

        string content;

        try
        {
            content = File.ReadAllText(vdfPath);
        }
        catch (Exception ex)
        {
            //Unsure about this
            LogSteamFailedToReadLibraryFoldersVdfLibrary(vdfPath, ex);
            return (libraries, "Failed to read Steam's libraryfolders.vdf");
        }

        foreach (Match match in RegexHelper.SteamLibraryRegex.Matches(content))
        {
            var libraryPath = GameScanHelper.NormalizePath(match.Groups[1].Value.Replace(@"\\", @"\"));

            if (Directory.Exists(libraryPath) && !libraries.Contains(libraryPath))
                libraries.Add(libraryPath);
        }

        return (libraries, null);
    }
}