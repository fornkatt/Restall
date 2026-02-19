using System.Collections.ObjectModel;
using Restall.Models;

namespace Restall.ViewModels;

public class GameListViewModel : ViewModelBase
{
    private Game? _selectedGame;

    //TODO: INSTALLED GAMES AND NOT INSTALLED GAMES OBSERVABLE COLLECTION?
    public ObservableCollection<Game> Games { get; } = [];

    public Game? SelectedGame
    {
        get => _selectedGame;
        set => SetProperty(ref _selectedGame, value);
    }

    public GameListViewModel()
    {
        Games.Add(new Game { Name = "asdf" });
        Games.Add(new Game { Name = "qwerty" });
        Games.Add(new Game { Name = "1234" });
        Games.Add(new Game { Name = "5678" });
        Games.Add(new Game { Name = "5678" });
        Games.Add(new Game { Name = "5678" });
        Games.Add(new Game { Name = "5678" });
        Games.Add(new Game { Name = "5678" });
        Games.Add(new Game { Name = "5678" });
        Games.Add(new Game { Name = "5678" });
        Games.Add(new Game { Name = "5678" });
        Games.Add(new Game { Name = "5678" });
        Games.Add(new Game { Name = "5678" });
        Games.Add(new Game { Name = "5678" });
        Games.Add(new Game { Name = "5678" });
        Games.Add(new Game { Name = "5678" });
        Games.Add(new Game { Name = "5678" });
        Games.Add(new Game { Name = "5678" });
        Games.Add(new Game { Name = "5678" });
        Games.Add(new Game { Name = "5678" });
    }
}