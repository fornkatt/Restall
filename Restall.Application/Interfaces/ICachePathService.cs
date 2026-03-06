using Restall.Domain.Entities;

namespace Restall.Application.Interfaces;

public interface ICachePathService
{
    public string GetRenoDXCachePath(RenoDX renoDx);
    public string GetReShadeCachePath(ReShade reShade);
}