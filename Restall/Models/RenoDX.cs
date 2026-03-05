using System;
using System.Collections.Generic;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Restall.Models;

#region ENUMS
//TODO: CHECK DISCORD?

#endregion

public class RenoDX : ObservableObject
{
    public enum Branch
    {
        Unknown,
        Snapshot, // GITHUB SNAPSHOT - DIRECT LINK
        Nightly, // GITHUB NIGHTLY - MATCH REGEX
        Discord, //OPTIONAL - ADD TO JSON
        Nexus //OPTIONAL - ADD TO JSON
    }
    
    public string? Name { get; set; }
    public string? Maintainer { get; set; } // MOD CREATOR
    
    public string? SnapShotUrl { get; set; }
    public string? SnapShotUrl32 { get; set; }
    public string? NightlyUrl { get; set; } 
    public string? DiscordUrl { get; set; } 
    public string? NexusUrl { get; set; }
    
    public Branch BranchName { get; set; } = Branch.Unknown;
    
    //DATEONLY AND CONVERT IT TO STRING, OR THE OTHER WAY AROUND
    public string? Version { get; set; }
    
    private bool _isInstalled;
    public bool IsInstalled
    {
        get => _isInstalled;
        set => SetProperty(ref _isInstalled, value);
    }
    
    public string GetCachePath()
    {
        return Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "Cache", "RenoDX", BranchName.ToString(), Name!
        );
    }
}