namespace Restall.Domain.Entities;

public class RenoDX
{
    public enum Branch { Unknown, Snapshot, Nightly, Discord, Nexus }
    public enum Architecture { x32 = 32, x64 = 64 }
    
    public string? Name { get; set; }
    public string? Maintainer { get; set; }
    public Branch BranchName { get; set; } = Branch.Unknown;
    public Architecture Arch { get; set; } = Architecture.x64;
    public string? Version { get; set; }
}