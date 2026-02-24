using Restall.Models;

namespace Restall.ViewModels;

public class ModViewModel : ViewModelBase
{
    private readonly MainWindowViewModel _mainWindowViewModel;
    private readonly AppState _appState;
    
    
    public Game? SelectedGame
    {
        get => _mainWindowViewModel.SelectedGame;
        set
        {
            if (_mainWindowViewModel.SelectedGame != value)
            {
                _mainWindowViewModel.SelectedGame = value;
                OnPropertyChanged();
            }
        }
    }
    
    //TODO: RELAYCOMMAND TO INSTALL, UPDATE AND DELETE RENODX AND RESHADE

    public ModViewModel(MainWindowViewModel mainWindowViewModel, AppState appState)
    {
        _appState = appState;
        _mainWindowViewModel = mainWindowViewModel;
        
        _mainWindowViewModel.PropertyChanged += (s, e) =>
        {
            switch (e.PropertyName)
            {
                case nameof(_mainWindowViewModel.SelectedGame):
                    OnPropertyChanged(nameof(SelectedGame));
                    break;
            }
        };
    }
    
}