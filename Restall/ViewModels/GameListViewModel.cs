using System.Collections.ObjectModel;
using Restall.Models;

namespace Restall.ViewModels;

public class GameListViewModel : ViewModelBase
{
    private readonly AppState _appState;
    private readonly MainWindowViewModel _mainWindowViewModel;

    public Game? SelectedGame
    {
        get => _mainWindowViewModel.SelectedGame;
        set => _mainWindowViewModel.SelectedGame = value;
    }
    public ObservableCollection<Game> Games => _mainWindowViewModel.Games;

    public GameListViewModel(MainWindowViewModel mainWindowViewModel, AppState appState)
    {
        _mainWindowViewModel = mainWindowViewModel;
        _appState = appState;
        
        _mainWindowViewModel.PropertyChanged += (s, e) =>
        {
            switch (e.PropertyName)
            {
                case nameof(_mainWindowViewModel.Games):
                case nameof(_mainWindowViewModel.SelectedGame):
                    OnPropertyChanged(nameof(SelectedGame));
                    break;
            }
        };
    }
}