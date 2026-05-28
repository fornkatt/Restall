namespace Restall.Domain.Entities;

public sealed class RenoDX
{
    public enum Branch { Unknown, Wiki, Snapshot, Nightly, Discord, Nexus }
    public enum Architecture { x32 = 32, x64 = 64 }
    
    public string? SelectedName { get; set; }
    public string? OriginalName { get; set; }
    public Branch BranchName { get; set; } = Branch.Unknown;
    public Architecture Arch { get; set; } = Architecture.x64;
    public string? Version { get; set; }

    public bool IsUpdateCheckSupported =>
        OriginalName is null || !OriginalName.StartsWith("renodx-unityengine", StringComparison.OrdinalIgnoreCase);
}