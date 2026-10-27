// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using PeNet;
using Restall.Infrastructure.DTOs;

namespace Restall.Infrastructure.Helpers;

internal static class PeVersionHelper
{
    private const int MzSignatureLength = 2;

    /// <summary>
    /// Get file information through PeNet.<br/><br/>
    /// Used, for instance, to get the original file name and file version back from a file using PE headers.
    /// </summary>
    internal static PeVersionInfo? GetVersionInfo(string filePath)
    {
        using var stream = File.OpenRead(filePath);

        if (stream.Length < MzSignatureLength || !PeFile.TryParse(stream, out var peFile) || peFile is null)
            return null;

        var stringTable = peFile.Resources?.VsVersionInfo?.StringFileInfo.StringTable.FirstOrDefault();

        return stringTable is null
            ? null
            : new PeVersionInfo(stringTable.OriginalFilename, stringTable.FileVersion,
                stringTable.ProductName, stringTable.ProductVersion);
    }
}
