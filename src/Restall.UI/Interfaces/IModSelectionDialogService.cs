// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Application.DTOs;
using Restall.UI.DTOs;
using System.Threading.Tasks;

namespace Restall.UI.Interfaces;

public interface IModSelectionDialogService
{
    Task<ReShadeInstallSelectionDto?> ShowReShadeInstallDialogAsync();
    Task<RenoDXTagInfoDto?> ShowRenoDXInstallDialogAsync();
}
