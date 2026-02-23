using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Restall.Models;

public class RenoDX : ObservableObject
{
    public string? Name { get; set; }
    
    //DATEONLY AND CONVERT IT TO STRING, OR THE OTHER WAY AROUND
    public string? Version { get; set; }
    public List<string> AvailableVersions { get; set; } = [];
    
    private bool _isInstalled;
    public bool IsInstalled
    {
        get => _isInstalled;
        set => SetProperty(ref _isInstalled, value);
    }
}