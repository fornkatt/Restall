// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Application.Helpers;

namespace Restall.Application.DTOs;

public sealed class PreparedName
{
    private PreparedName(string wordSplitName, string normalizedName)
    {
        WordSplitName = wordSplitName;
        NormalizedName = normalizedName;
    }

    public string WordSplitName { get; }
    public string NormalizedName { get; }

    public static PreparedName Create(string name)
    {
        var wordSplitName = GameNameHelper.SplitInWordSeparators(GameNameHelper.RemoveLatinAccents(name));
        var normalizedName = GameNameHelper.NormalizeName(wordSplitName);

        return new PreparedName(wordSplitName, normalizedName);
    }
}
