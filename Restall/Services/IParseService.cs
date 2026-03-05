using System.Collections.Generic;
using System.Threading.Tasks;
using Restall.Models;

namespace Restall.Services;

public interface IParseService
{
    Task FetchAvailableModVersionsAsync();
}