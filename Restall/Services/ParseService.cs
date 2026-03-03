using System.Collections.Generic;
using System.Threading.Tasks;

namespace Restall.Services;

public class ParseService : IParseService
{
    public async Task<(List<string> Versions, Dictionary<string, string> UEGenericDescription)> FetchAllRenoDXVersionsAsync()
    {
        throw new System.NotImplementedException();
    }

    public async Task<List<string>> FetchAllReShadeVersionsAsync()
    {
        throw new System.NotImplementedException();
    }
}