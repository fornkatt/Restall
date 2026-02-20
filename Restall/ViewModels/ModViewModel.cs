namespace Restall.ViewModels;

public class ModViewModel : ViewModelBase
{
    private readonly MainWindowViewModel _mainWindowViewModel;
    private readonly AppState _appState;

    public ModViewModel(MainWindowViewModel mainWindowViewModel, AppState appState)
    {
        _appState = appState;
        _mainWindowViewModel = mainWindowViewModel;
    }
}