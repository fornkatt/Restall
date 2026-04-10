namespace Restall.Domain.Entities;

public sealed class ReShade
{
    public enum Branch { Unknown, Stable, Nightly, RenoDX }
    public enum Filename { Dxgi, D3d12, D3d11, D3d10, D3d9, Version, ReShade32, ReShade64 }
    public enum FileExtension { Dll, Asi }
    public enum Architecture { x32 = 32, x64 = 64 }

    public Architecture Arch { get; set; } = Architecture.x64;

    public static readonly IReadOnlyDictionary<Filename, string> FullFileName =
        new Dictionary<Filename, string>
        {
            [Filename.Dxgi] = "dxgi",
            [Filename.D3d12] = "d3d12",
            [Filename.D3d11] = "d3d11",
            [Filename.D3d10] = "d3d10",
            [Filename.D3d9] = "d3d9",
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