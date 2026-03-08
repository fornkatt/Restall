using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Restall.Application.Interfaces;
using Restall.Domain.Entities;
using Restall.UI.Messages;

namespace Restall.UI.ViewModels;

public partial class MainWindowViewModel : ViewModelBase, IRecipient<SelectedGameChangedMessage>
{
    private readonly ILogService _logService;
    private readonly IGameDetectionService _gameDetectionService;
    private readonly IModDetectionService _modDetectionService;
    private readonly IParseService _parseService;
    private readonly IModInstallService _modInstallService;
    private bool _suppressMessage;

    public BannerViewModel BannerViewModel { get; }
    public GameListViewModel GameListViewModel { get; }
    public ModViewModel ModViewModel { get; }

    [ObservableProperty]
    private GameModViewModel? _selectedGame;

    partial void OnSelectedGameChanged(GameModViewModel? value)
    {
        OnPropertyChanged(nameof(IsGameSelected));

        if (!_suppressMessage)
            Messenger.Send(new SelectedGameChangedMessage(value));
    }

    public bool IsGameSelected => SelectedGame != null;

    //TODO: INSTALLED GAMES AND NOT INSTALLED GAMES OBSERVABLE COLLECTION?

    public MainWindowViewModel(
        ILogService logService,
        IGameDetectionService gameDetectionService,
        IModDetectionService modDetectionService,
        IParseService parseService,
        IModInstallService modInstallService,
        BannerViewModel bannerViewModel,
        GameListViewModel gameListViewModel,
        ModViewModel modViewModel
        )
    {
        _logService = logService;
        _gameDetectionService = gameDetectionService;
        _modDetectionService = modDetectionService;
        _parseService = parseService;
        _modInstallService = modInstallService;

        BannerViewModel = bannerViewModel;
        GameListViewModel = gameListViewModel;
        ModViewModel = modViewModel;

        IsActive = true;

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

        //TODO: JUST TESTING PURPOSES WITH ASYNC METHODS, FIRE AND FORGET WILL BE REMOVED!
        _ = InitializeAsync();

        // _ = modDetectionService.DetectInstalledReShadeAsync(testGame.ExecutablePath);
        // _ = modDetectionService.DetectInstalledRenoDXAsync(testGame.ExecutablePath);


        // _ = modInstallService.InstallModAsync(testGame, testReShade);
        _ = _modInstallService.RemoveAllReShadeFiles(testGame.GetGame());
        _ = _modInstallService.RemoveAllRenoDXFiles(testGame.GetGame());


    }

    public void Receive(SelectedGameChangedMessage message)
    {
        _suppressMessage = true;
        SelectedGame = message.Value;
        _suppressMessage = false;
    }


    private async Task InitializeAsync()
    {
        var gamesTask = _gameDetectionService.FindGames();
        var modsTask = _parseService.FetchAvailableModVersionsAsync();

        await Task.WhenAll(gamesTask, modsTask);

        var sortedGames = gamesTask.Result
            .Where(g => g != null)
            .OrderBy(g => g!.Name);

        foreach (var game in sortedGames)
        {
            var vm = new GameModViewModel(game!)
            {
                CompatibleRenoDXMod = _parseService.GetCompatibleRenoDXMod(game!.Name)
            };

            if (vm.CompatibleRenoDXMod is null)
            {
                vm.CompatibleRenoDXGenericMod = _parseService.GetGenericRenoDXInfo(game!.Name);
            }

            GameListViewModel.Games.Add(vm);

            if (vm.CompatibleRenoDXMod is not null)
                _logService.LogInfo($"Found compatible RenoDX game: {vm.Name}");

            if (vm.CompatibleRenoDXGenericMod is not null)
                _logService.LogInfo($"Found compatible engine generic RenoDX game: {vm.Name}");
        }
    }
}