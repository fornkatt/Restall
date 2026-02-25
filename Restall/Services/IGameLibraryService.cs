using System.Threading.Tasks;

namespace Restall.Services;

public interface IGameLibraryService
{
    Task LoadAsync();
    Task SaveAsync();
}