namespace Restall.Domain.Entities;

public class ReShade
{
    public enum Branch { Unknown, Stable, Nightly, RenoDX }
    public enum FileName { Dxgi, D3d12, D3d11, Version, ReShade32, ReShade64 }
    public enum FileExtension { Dll, Asi }
    public enum Architecture { x32 = 32, x64 = 64 }

    public Architecture Arch { get; set; } = Architecture.x64;

    public static readonly IReadOnlyDictionary<FileName, string> FullFileName =
        new Dictionary<FileName, string>
    {
        [FileName.Dxgi] = "dxgi",
        [FileName.D3d12] = "d3d12",
        [FileName.D3d11] = "d3d11",
        [FileName.Version] = "version",
        [FileName.ReShade32] = "ReShade32",
        [FileName.ReShade64] = "ReShade64"
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

    public static string GetFileName(FileName fileType, FileExtension extension)
    {
        return $"{FullFileName[fileType]}{Extension[extension]}";
    }
}