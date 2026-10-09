// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using System.Threading.Tasks;
namespace Restall.UI.Interfaces;

public interface IFolderPickerService
{
    Task<string?> PickFolderAsync();
}
