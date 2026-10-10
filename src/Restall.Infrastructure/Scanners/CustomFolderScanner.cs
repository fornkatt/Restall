// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Microsoft.Extensions.Logging;
using Restall.Application.DTOs.Responses;
using Restall.Application.Interfaces.Driven;
using Restall.Domain.Entities;
using Restall.Infrastructure.Helpers;

namespace Restall.Infrastructure.Scanners;

internal sealed partial class CustomFolderScanner : IPlatformScannerService
{
    private readonly ICustomGameFolderProvider _folderProvider;
    private readonly ILogger<CustomFolderScanner> _logger;
    public CustomFolderScanner(ICustomGameFolderProvider folderProvider, ILogger<CustomFolderScanner> logger)
    {
        _folderProvider = folderProvider;
        _logger = logger;
    }
    public Game.Platform Platform => Game.Platform.Custom;
    public Task<GameScanResultDto> ScanAsync() => Task.Run(ScanCustomFolders);

    private GameScanResultDto ScanCustomFolders()
    {
        var games = new List<Game>();

        foreach (var root in _folderProvider.GetFolders())
        {
            if (!Directory.Exists(root))
            {
                LogCustomGameFolderNotFound(root);
                continue;
            }

            foreach (var sub in Directory.EnumerateDirectories(root))
            {
                var name = Path.GetFileName(sub);
                if (GameScanHelper.NonGame(name)) continue;

                games.Add(new Game
                {
                    Name = name,
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
