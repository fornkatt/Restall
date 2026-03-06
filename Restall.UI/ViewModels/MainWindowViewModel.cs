using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Restall.Application.Interfaces;
using Restall.Domain.Entities;
namespace Restall.UI.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IGameDetectionService _gameDetectionService;
    private readonly IModDetectionService _modDetectionService;
    private readonly IParseService _parseService;

    public BannerViewModel BannerViewModel { get; }
    public GameListViewModel GameListViewModel { get; }
    public ModViewModel ModViewModel { get; }
    
    [ObservableProperty]
    private ObservableCollection<GameModViewModel> _games = [];
    [ObservableProperty]
    private ObservableCollection<GameModViewModel> _installedGames = [];
    [ObservableProperty]
    private GameModViewModel? _selectedGame;
    
    partial void OnSelectedGameChanged(GameModViewModel? value)
    {
        OnPropertyChanged(nameof(IsGameSelected));
    }
    
    public bool IsGameSelected => SelectedGame != null;

    //TODO: INSTALLED GAMES AND NOT INSTALLED GAMES OBSERVABLE COLLECTION?

    public MainWindowViewModel(
        IGameDetectionService gameDetectionService,
        IModDetectionService modDetectionService,
        IParseService parseService
        )
    {
        _gameDetectionService = gameDetectionService;
        _modDetectionService = modDetectionService;
        _parseService = parseService;

        Debug.WriteLine(AppDomain.CurrentDomain.BaseDirectory);

        BannerViewModel = new(this);
        GameListViewModel = new(this);
        ModViewModel = new(this);
        
        //TODO: JUST TESTING PURPOSES WITH ASYNC METHODS, FIRE AND FORGET WILL BE REMOVED!
        _ = InitializeAsync();

        ReShade testReShade = new()
        {
            SelectedFileName = new ReShade().GetFileName(ReShade.FileName.D3d11, ReShade.FileExtension.Dll),
            BranchName = ReShade.Branch.Stable,
            Version = "6.7.2",
            Arch = ReShade.Architecture.x64
        };
        
        var testGame = new GameModViewModel(new Game
        {
            Name = "Batman™: Arkham Knight",
            EngineName = Game.Engine.Unreal,
            ExecutablePath = @"D:\Games\Steam\steamapps\common\Batman Arkham Knight\Binaries\Win64\",
            InstallFolder = @"D:\Games\Steam\steamapps\common\Batman Arkham Knight\",
            IsInstalled = true,
            PlatformName = Game.Platform.Steam,
            RenoDX = null,
            ReShade = testReShade,
        });

        // _ = modDetectionService.DetectInstalledReShadeAsync(testGame.ExecutablePath);
        // _ = modDetectionService.DetectInstalledRenoDXAsync(testGame.ExecutablePath);
        _ = _parseService.FetchAvailableModVersionsAsync();

        // _ = modInstallService.InstallModAsync(testGame, testReShade);
        // _ = modInstallService.RemoveOtherReShadeFiles(testGame);
    }


    private async Task InitializeAsync()
    {
        var games = await _gameDetectionService.FindGames();

        foreach (var game in games)
        {
            if (game == null) continue;
            Games.Add(new GameModViewModel(game));
        }
    }
}