namespace Restall.Domain.Entities;

public class RenoDX
{
    public enum Branch { Unknown, Snapshot, Nightly, Discord, Nexus }
    
    public string? Name { get; set; }
    public string? Maintainer { get; set; }
    public Branch BranchName { get; set; } = Branch.Unknown;
    public string? Version { get; set; }
}