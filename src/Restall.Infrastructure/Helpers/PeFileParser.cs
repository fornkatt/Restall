// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using PeNet;

namespace Restall.Infrastructure.Helpers;

internal static class PeFileParser
{
    private const int MzSignatureLength = 2;

    internal static PeFile? Parse(Stream stream)
    {
        if (stream.Length < MzSignatureLength || !PeFile.TryParse(stream, out var peFile))
            return null;

        return peFile;
    }
}
