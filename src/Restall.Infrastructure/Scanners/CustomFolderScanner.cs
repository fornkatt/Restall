// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Application.Interfaces.Driven;
using Restall.Application.DTOs.Results;
using Restall.Domain.Entities;

namespace Restall.Infrastructure.Scanners;

internal sealed partial class CustomFolderScanner : IPlatformScannerService
{
    private readonly ICustomGameFolderProvider _folderProvider;
    public CustomFolderScanner(ICustomGameFolderProvider folderProvider) => _folderProvider = folderProvider;

    public Game.Platform Platform => Game.Platform.Custom;
    public Task<GameScanResultDto> ScanAsync() => Task.Run(ScanCustomFolders);

    private GameScanResultDto ScanCustomFolders()
    {
        var games = new List<Game>();
        foreach (var root in _folderProvider.GetFolders())
        {
            // Only direct subfolders count as games. EngineDetectionService goes deeper for the exe later
            foreach (var sub in Directory.EnumerateDirectories(root))
            {
                // PlatformId is left null so a Steam/Epic/etc. entry for the same folder is kept during deduplication.
                games.Add(new Game
                {
                    Name = Path.GetFileName(sub),
                    InstallFolder = sub,
                    PlatformName = Platform
                });
            }
        }

        return new GameScanResultDto(
            Platform: Platform,
            Games: games,
            IsSuccess: games.Count > 0,
            Message: null);
    }
}
