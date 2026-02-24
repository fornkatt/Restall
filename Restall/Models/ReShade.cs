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