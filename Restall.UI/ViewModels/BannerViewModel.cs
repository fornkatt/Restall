using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Restall.UI.Messages;

namespace Restall.UI.ViewModels;

public partial class BannerViewModel : ViewModelBase, IRecipient<SelectedGameChangedMessage>
{
    [ObservableProperty]
    private GameModViewModel? _selectedGame;

    public void Receive(SelectedGameChangedMessage message) => SelectedGame = message.Value;

    public BannerViewModel() => IsActive = true;
}