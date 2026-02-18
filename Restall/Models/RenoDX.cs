using CommunityToolkit.Mvvm.ComponentModel;

namespace Restall.Models;

public class RenoDX : ObservableObject
{
    public string? Name { get; set; }
    public string? Version { get; set; }
    public string? AvailableVersion { get; set; }
    
    private bool _isInstalled;
    public bool IsInstalled
    {
        get => _isInstalled;
        set => SetProperty(ref _isInstalled, value);
    }
}