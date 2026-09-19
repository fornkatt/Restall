// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Restall.Domain.Entities;

public sealed class ReShade
{
    public enum Branch { Unknown, Stable, Nightly, RenoDX }
    public enum Filename { Dxgi, D3D12, D3D11, D3D10, D3D9, Version, ReShade32, ReShade64 }
    public enum FileExtension { Dll, Asi }
    public enum Architecture { X32 = 32, X64 = 64 }

    public Architecture Arch { get; set; } = Architecture.X64;

    public static readonly IReadOnlyDictionary<Filename, string> FullFileName =
        new Dictionary<Filename, string>
        {
            [Filename.Dxgi] = "dxgi",
            [Filename.D3D12] = "d3d12",
            [Filename.D3D11] = "d3d11",
            [Filename.D3D10] = "d3d10",
            [Filename.D3D9] = "d3d9",
            [Filename.Version] = "version",
            [Filename.ReShade32] = "ReShade32",
            [Filename.ReShade64] = "ReShade64"
        };

    public static readonly IReadOnlyDictionary<FileExtension, string> Extension =
        new Dictionary<FileExtension, string>
        {
            [FileExtension.Dll] = ".dll",
            [FileExtension.Asi] = ".asi"
        };

    public Branch BranchName { get; set; } = Branch.Unknown;
    public string OriginalFileName => $"ReShade{(int)Arch}.dll";
    public string SelectedFilename { get; set; } = string.Empty;
    public string? Version { get; set; }

    public static string GetFileName(Filename fileType, FileExtension extension)
    {
        return $"{FullFileName[fileType]}{Extension[extension]}";
    }
}