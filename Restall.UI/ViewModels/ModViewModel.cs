using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Restall.UI.Messages;

namespace Restall.UI.ViewModels;

public partial class ModViewModel : ViewModelBase, IRecipient<SelectedGameChangedMessage>
{
    private bool _suppressMessage;

    [ObservableProperty]
    private GameModViewModel? _selectedGame;

    [ObservableProperty]
    private int _downloadPercent;

    [ObservableProperty]
    private string? _downloadStatus;

    //TODO: RELAYCOMMAND TO INSTALL, UPDATE AND DELETE RENODX AND RESHADE

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

    public ModViewModel() => IsActive = true;

    /* Implement this to prompt for a call on a deepscan if the expected ReShade file held by the Game.ReShade object was not found */

    // var result = await modInstallService.UninstallModAsync(SelectedGame, SelectedGame.ReShade);
    //
    //     if (result.ShouldPromptForDeepScan)
    // {
    //     var userConfirmed = await ShowConfirmationDialog(
    //         "ReShade file not found at expected location. Would you like to scan for and remove other ReShade installations?");
    //
    //     if (userConfirmed)
    //     {
    //         result.UpdatedGame = await modInstallService.RemoveOtherReShadeFiles(result.UpdatedGame);
    //     }
    // }
    //
    // SelectedGame = result.UpdatedGame;
}