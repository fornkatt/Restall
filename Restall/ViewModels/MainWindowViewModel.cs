using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using Restall.Models;
using Restall.Services;

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
        var reshade001 = new ReShade {Version = "6.7.2", AvailableVersions =  { "1.0.0",  "1.0.1" }, IsInstalled = true};
        var reshade002 = new ReShade {Version = "6.7.0", AvailableVersions = { "3.0.1", "3.1.3" },IsInstalled = false};
        var reshade003 = new ReShade {Version = "6.6.8", AvailableVersions = {"0.9.3", "0.9.8"}, IsInstalled = false};
        var renoDX001 = new RenoDX{Name = "renodx-cronosthenewdawn", Version = "3.4.5", AvailableVersions = {"","" },IsInstalled = true};
        var renoDX002 = new RenoDX{Name = "renodx-silenthill2remake", Version = "3.2.1",AvailableVersions = {"","" },IsInstalled = false };
        var renoDX003 = new RenoDX{Name = "renodx-clairobscur_expedition33", Version = "5.3.2",AvailableVersions = {"","" },IsInstalled = false };
        
        Games.Add(new Game { PlatformName = Game.Platform.GOG, Name = "Cronos: The New Dawn", BannerPathString = Path.Combine("Assets", "test_game_01", "test_banner.jpg"),
            ThumbnailPathString = Path.Combine("Assets", "test_game_01", "test_icon.png"),
            LogoPathString = Path.Combine("Assets", "test_game_01", "test_logo.png"),
            ReShade = reshade001, RenoDX = renoDX001
        });
        
        Games.Add(new Game { PlatformName = Game.Platform.Steam, Name = "Silent Hill 2 Remake", BannerPathString = Path.Combine("Assets", "test_game_02", "test_banner.png"),
            ThumbnailPathString = Path.Combine("Assets", "test_game_02", "test_icon.png"), 
            LogoPathString = Path.Combine("Assets", "test_game_02", "test_logo.png"),
            ReShade = reshade002, RenoDX = renoDX002,
        });
        
        Games.Add(new Game { Name = "Clair Obscur: Expedition 33", BannerPathString = Path.Combine("Assets", "test_game_03", "test_banner.png"),
            ThumbnailPathString = Path.Combine("Assets", "test_game_03", "test_icon.png"),
            LogoPathString = Path.Combine("Assets", "test_game_03", "test_logo.png"),
            ReShade = reshade003, RenoDX = renoDX003,
        });
        
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

        var fileExtractionService = new FileExtractionService(new LogService());
        fileExtractionService.ExtractFiles();
        
        System.Diagnostics.Debug.WriteLine("Game list view model loaded");
        
        Game testGame = new()
        {
            Name = "Batman™: Arkham Knight",
            EngineName = Game.Engine.Unreal,
            ExecutableName = "BatmanAK.exe",
            ExecutablePath = @"D:\Games\Steam\steamapps\common\Batman Arkham Knight\Binaries\Win64\",
            InstallFolder = @"D:\Games\Steam\steamapps\common\Batman Arkham Knight\",
            IsInstalled = true,
            PlatformName = Game.Platform.Steam,
            RenoDX = null,
            ReShade = null,
        };
        
        
    }
}