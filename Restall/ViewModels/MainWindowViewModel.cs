using CommunityToolkit.Mvvm.ComponentModel;

namespace Restall.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    // public GameListViewModel _gameListViewModel { get; } 
    // public ModViewModel  _modViewModel { get; }

    public BannerViewModel BannerViewModel { get; }
    public GameListViewModel GameListViewModel { get; }
    public ModViewModel ModViewModel { get; }

    public MainWindowViewModel()
    {
        
        BannerViewModel = new();
        GameListViewModel = new();
        ModViewModel = new();
    }

    

    

    // public GameListViewModel GameListViewModel
    // {
    //     get => _gameListViewModel;
    //     set => SetProperty(ref _gameListViewModel, value);  
    // }
    // public ModViewModel ModViewModel
    // {
    //     get => _modViewModel;
    //     set => SetProperty(ref _modViewModel, value);
    // }
    
}