namespace Restall.Domain.Entities;

public class ReShade
{
    public enum Branch { Unknown, Stable, Nightly, RenoDX }
    public enum FileName { Dxgi, D3d12, D3d11, Version }
    public enum FileExtension { Dll, Asi }
    public enum Architecture { x32 = 32, x64 = 64 }

    public Architecture Arch { get; set; } = Architecture.x64;

    public Dictionary<FileName, string> FullFileName => new()
    {
        [FileName.Dxgi] = "dxgi",
        [FileName.D3d12] = "d3d12",
        [FileName.D3d11] = "d3d11",
        [FileName.Version] = "version"
    };

    public Dictionary<FileExtension, string> Extension => new()
    {
        [FileExtension.Dll] = ".dll",
        [FileExtension.Asi] = ".asi"
    };

    public Branch BranchName { get; set; } = Branch.Unknown;
    public string OriginalFileName => $"ReShade{(int)Arch}.dll";
    public string SelectedFileName { get; set; } = string.Empty;
    public string? Version { get; set; }

    public string GetFileName(FileName fileType, FileExtension extension)
    {
        return $"{FullFileName[fileType]}{Extension[extension]}";
    }
}