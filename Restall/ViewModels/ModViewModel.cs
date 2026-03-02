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
    
    /* Implement this to prompt for a call on a deepscan if the expected ReShade file held by the Game.ReShade object was not found */
    
    // var result = await modInstallService.UninstallModAsync(SelectedGame, SelectedGame.ReShade);
    //
    //     if (result.ShouldPromptForDeepScan)
    // {
    //     var userConfirmed = await ShowConfirmationDialog(
    //         "ReShade file not found at expected location. Would you like to scan for and remove other ReShade installations?");
    //
    //     if (userConfirmed)
    //     {
    //         result.UpdatedGame = await modInstallService.RemoveOtherReShadeFiles(result.UpdatedGame);
    //     }
    // }
    //
    // SelectedGame = result.UpdatedGame;
}