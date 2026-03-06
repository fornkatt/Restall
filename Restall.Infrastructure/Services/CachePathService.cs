using Restall.Application.Interfaces;
using Restall.Domain.Entities;

namespace Restall.Infrastructure.Services;

public class CachePathService : ICachePathService
{
    public string GetRenoDXCachePath(RenoDX renoDx)
    {
        return Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "Cache", "RenoDX", renoDx.BranchName.ToString(), renoDx.Name!
        );
    }

    public string GetReShadeCachePath(ReShade reShade)
    {
        return Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "Cache", "ReShade", reShade.BranchName.ToString(), reShade.Version!
        );
    }
}