using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Restall.UI.Messages;
using System.Collections.ObjectModel;

namespace Restall.UI.ViewModels;

public partial class GameListViewModel : ViewModelBase, IRecipient<SelectedGameChangedMessage>
{
    private bool _suppressMessage;

    [ObservableProperty]
    private ObservableCollection<GameModViewModel> _games = [];

    [ObservableProperty]
    private GameModViewModel? _selectedGame;

    partial void OnSelectedGameChanged(GameModViewModel? value)
    {
        if (!_suppressMessage)
            Messenger.Send(new SelectedGameChangedMessage(value));
    }

    public void Receive(SelectedGameChangedMessage message)
    {
        _suppressMessage = true;
        SelectedGame = message.Value;
        _suppressMessage = false;
    }

    public GameListViewModel() => IsActive = true;
}