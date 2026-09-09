// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Domain.Entities;

namespace Restall.Application.Interfaces.Driven;

public interface IEngineDetectionService
{
    public (string? executablePath, Game.Engine engine) DetectExecutablePathAndEngine(string rootPath, Game.Platform platform);

}