using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Restall.Application.DTOs;
using Restall.Application.Interfaces;
using Restall.UI.Messages;

namespace Restall.UI.ViewModels;

public partial class MainWindowViewModel : ViewModelBase, IRecipient<SelectedGameChangedMessage>
{
    private bool _suppressMessage;

    public BannerViewModel BannerViewModel { get; }
    public GameListViewModel GameListViewModel { get; }
    public ModViewModel ModViewModel { get; }

    [ObservableProperty]
    private GameModViewModel? _selectedGame;

    [ObservableProperty]
    private bool _isInitializing = true;

    [ObservableProperty]
    private string _initializationMessage = "Scanning for games...";

    public bool IsGameSelected => SelectedGame != null;

    public MainWindowViewModel(
        BannerViewModel bannerViewModel,
        GameListViewModel gameListViewModel,
        ModViewModel modViewModel
        )
    {
        BannerViewModel = bannerViewModel;
        GameListViewModel = gameListViewModel;
        ModViewModel = modViewModel;

        IsActive = true;
    }

    public void LoadGames(AppInitializationResultDto result)
    {
        foreach (var item in result.Games)
        {
            GameListViewModel.Games.Add(new GameModViewModel(item.Game)
            {
                CompatibleRenoDXMod = item.CompatibleMod,
                CompatibleRenoDXGenericMod = item.CompatibleGenericMod
            });
        }
    }

    partial void OnSelectedGameChanged(GameModViewModel? value)
    {
        OnPropertyChanged(nameof(IsGameSelected));

        if (!_suppressMessage)
            Messenger.Send(new SelectedGameChangedMessage(value));
    }

    public void Receive(SelectedGameChangedMessage message)
    {
        _suppressMessage = true;
        SelectedGame = message.Value;
        _suppressMessage = false;
    }
}