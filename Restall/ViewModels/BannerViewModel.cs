using Restall.Models;

namespace Restall.ViewModels;

public class BannerViewModel : ViewModelBase
{
    private readonly AppState _appState;
    private readonly MainWindowViewModel _mainWindowViewModel;

    public Game? SelectedGame => _mainWindowViewModel.SelectedGame;
    
    
    public BannerViewModel(MainWindowViewModel mainWindowViewModel, AppState appState)
    {
        _mainWindowViewModel = mainWindowViewModel;
        _appState = appState;

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