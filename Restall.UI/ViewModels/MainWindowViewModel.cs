using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Restall.UI.Messages;

namespace Restall.UI.ViewModels;

/// <summary>
/// We use the MainWindowViewModel as a mediator between its children (light mediator pattern)
/// Messages go to this ViewModel and it sends them the children when something changes
/// intead of having the children talk to each other directly.
/// </summary>
public sealed partial class MainWindowViewModel : ViewModelBase,
    IRecipient<SelectedGameChangedMessage>,
    IRecipient<WikiRefreshedMessage>
{
    public BannerViewModel BannerViewModel { get; }
    public GameListViewModel GameListViewModel { get; }
    public ModViewModel ModViewModel { get; }

    [ObservableProperty]
    private GameModViewModel? _selectedGame;

    public bool IsGameSelected => SelectedGame is not null;

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


    partial void OnSelectedGameChanged(GameModViewModel? value)
    {
        OnPropertyChanged(nameof(IsGameSelected));

        GameListViewModel.ApplySelectedGame(value);
        ModViewModel.ApplySelectedGame(value);
        BannerViewModel.ApplySelectedGame(value);
    }

    public void Receive(SelectedGameChangedMessage message) => SelectedGame = message.Value;

    public void Receive(WikiRefreshedMessage message) => ModViewModel.ApplyWikiRefresh();
}