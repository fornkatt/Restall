// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Restall.Application.Interfaces.Driven;

public interface IGameIconService
{
    Task ExtractIconIfMissingAsync(string? executablePath, string? gameName, string iconPath);
}
