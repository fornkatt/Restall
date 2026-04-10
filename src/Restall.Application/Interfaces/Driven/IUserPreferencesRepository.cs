namespace Restall.Application.Interfaces.Driven;

/// <summary>
/// To be used in a later implementation to allow the user to save preferences to a JSON file.
/// For example, dark/light mode, preferred mod source (see RenoDXModPreferenceDto) etc.
/// </summary>
public interface IUserPreferencesRepository
{
    void SaveAsync();
    void LoadAsync();
}