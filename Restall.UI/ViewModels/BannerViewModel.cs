using CommunityToolkit.Mvvm.ComponentModel;

namespace Restall.UI.ViewModels;

public sealed partial class BannerViewModel : ViewModelBase
{
    [ObservableProperty]
    private GameModViewModel? _selectedGame;

    public void ApplySelectedGame(GameModViewModel? value) => SelectedGame = value;
}