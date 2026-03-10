namespace Restall.Application.Interfaces;

public interface IUserPreferencesRepository
{
    void SaveAsync();
    void LoadAsync();
}