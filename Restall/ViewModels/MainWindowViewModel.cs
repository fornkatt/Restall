using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using Restall.Models;

namespace Restall.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    protected AppState AppState { get; }
    public BannerViewModel BannerViewModel { get; }
    public GameListViewModel GameListViewModel { get; }
    public ModViewModel ModViewModel { get; }
    
    [ObservableProperty]
    private ObservableCollection<Game> _games = [];
    [ObservableProperty]
    private ObservableCollection<Game> _installedGames = [];
    [ObservableProperty]
    private ObservableCollection<Game> _notInstalledGames = [];
    [ObservableProperty]
    private Game? _selectedGame;

    partial void OnSelectedGameChanged(Game? value)
    {
        OnPropertyChanged(nameof(IsGameSelected));
    }
    
    public bool IsGameSelected => SelectedGame != null;

    //TODO: INSTALLED GAMES AND NOT INSTALLED GAMES OBSERVABLE COLLECTION?

    public MainWindowViewModel(AppState appState)
    {
        AppState = appState;
        BannerViewModel = new(this, AppState);
        GameListViewModel = new(this, AppState);
        ModViewModel = new(this, AppState);
        
        Games.Add(new Game { Name = "test_game_01", BannerPathString = Path.Combine("Assets", "test_game_01", "test_banner.jpg"),
            ThumbnailPathString = Path.Combine("Assets", "test_game_01", "test_icon.png"), LogoPathString = Path.Combine("Assets", "test_game_01", "test_logo.png") });
        
        Games.Add(new Game { Name = "test_game_02", BannerPathString = Path.Combine("Assets", "test_game_02", "test_banner.png"),
            ThumbnailPathString = Path.Combine("Assets", "test_game_02", "test_icon.png"), LogoPathString = Path.Combine("Assets", "test_game_02", "test_logo.png") });
        
        Games.Add(new Game { Name = "test_game_03", BannerPathString = Path.Combine("Assets", "test_game_03", "test_banner.png"),
            ThumbnailPathString = Path.Combine("Assets", "test_game_03", "test_icon.png"), LogoPathString = Path.Combine("Assets", "test_game_03", "test_logo.png") });
        
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
        
        System.Diagnostics.Debug.WriteLine("Game list view model loaded");
    }
}