using System.Collections.Generic;
using System.Threading.Tasks;
using Restall.Models;

namespace Restall.Services;

public interface IGameDetectionService
{
    Task<List<Game?>> FindGames();
}