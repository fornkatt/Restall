using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Restall.Models;

public class ReShade : ObservableObject
{
    public enum Branch
    {
        Unknown,
        Stable, // WEBPAGE - DIRECT LINK
        Nightly, // GITHUB WORKFLOW - ADD TO REGEX
        RenoDX
    }

    public enum FileName
    {
        Dxgi,
        ReShade,
        D3d12,
        D3d11,
        Version
    }

    public enum FileExtension
    {
        Dll,
        Asi
    }

    public Dictionary<FileName, string> FullFileName => new()
    {
        [FileName.Dxgi] = "dxgi",
        [FileName.D3d12] = "D3D12",
        [FileName.D3d11] = "D3D11",
        [FileName.ReShade] = "ReShade",
        [FileName.Version] = "version"
    };

    public Dictionary<FileExtension, string> Extension => new()
    {
        [FileExtension.Dll] = ".dll",
        [FileExtension.Asi] = ".asi"
    };

    public Branch BranchName { get; set; } = Branch.Unknown;
    
    public string? Version { get; set; }
    public string? StableUrl { get; set; }
    public string? NightlyUrl { get; set; }
    public string? RenoDXUrl { get; set; }
    
    public List<string> AvailableVersions { get; set; } = [];
    
    
    private bool _isInstalled;
    public bool IsInstalled
    {
        get => _isInstalled;
        set => SetProperty(ref _isInstalled, value);
    }
}