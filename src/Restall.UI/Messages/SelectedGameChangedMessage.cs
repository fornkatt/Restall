using CommunityToolkit.Mvvm.Messaging.Messages;
using Restall.UI.ViewModels;

namespace Restall.UI.Messages;

public sealed class SelectedGameChangedMessage(GameModViewModel? value) : ValueChangedMessage<GameModViewModel?>(value);