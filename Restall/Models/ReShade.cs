using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Restall.Models;

public class ReShade : ObservableObject
{
    
    public string? Version { get; set; }
    public List<string> AvailableVersions { get; set; } = [];
    
    private bool _isInstalled;
    public bool IsInstalled
    {
        get => _isInstalled;
        set => SetProperty(ref _isInstalled, value);
    }
}