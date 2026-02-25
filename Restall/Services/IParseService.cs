using System.Collections.Generic;
using System.Threading.Tasks;
using Restall.Models;

namespace Restall.Services;

public interface IParseService
{
    Task<(List<string> Versions, Dictionary<string, string> UEGenericDescription)> FetchAllRenoDXVersionsAsync();
    Task<List<string>> FetchAllReShadeVersionsAsync();
}