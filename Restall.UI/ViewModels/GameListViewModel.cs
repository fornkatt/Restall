using System.Collections.ObjectModel;

namespace Restall.UI.ViewModels;

public class GameListViewModel : ViewModelBase
{
    private readonly MainWindowViewModel _mainWindowViewModel;

    public GameModViewModel? SelectedGame
    {
        get => _mainWindowViewModel.SelectedGame;
        set
        {
            if (_mainWindowViewModel.SelectedGame != value)
            {
                _mainWindowViewModel.SelectedGame = value;
            }
        }
    }
    public ObservableCollection<GameModViewModel> Games => _mainWindowViewModel.Games;

    public GameListViewModel(MainWindowViewModel mainWindowViewModel)
    {
        _mainWindowViewModel = mainWindowViewModel;
        
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