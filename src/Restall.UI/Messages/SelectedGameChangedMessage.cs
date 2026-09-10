// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell
// SPDX-License-Identifier: GPL-3.0-or-later

using CommunityToolkit.Mvvm.Messaging.Messages;
using Restall.UI.ViewModels;

namespace Restall.UI.Messages;

public sealed class SelectedGameChangedMessage(GameModViewModel? value) : ValueChangedMessage<GameModViewModel?>(value);
