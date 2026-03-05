namespace Restall.Application.Interfaces;

public interface IGameLibraryService
{
    Task LoadAsync();
    Task SaveAsync();
}