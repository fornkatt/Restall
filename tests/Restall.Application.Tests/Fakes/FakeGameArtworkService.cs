// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Application.Interfaces.Driven;
using Restall.Domain.Entities;

namespace Restall.Application.Tests.Fakes;

public sealed class FakeGameArtworkService : IGameArtworkService
{
    public Task EnrichGameArtworkAsync(Game game) => Task.CompletedTask;
}
