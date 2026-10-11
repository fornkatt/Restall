// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Application.DTOs.RenoDXDTOs;
using Restall.UI.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Restall.UI.Interfaces;

public interface IModSelectionDialogService
{
    Task<ReShadeInstallSelectionDto?> ShowReShadeInstallDialogAsync();
    Task<RenoDXTagInfo?> ShowRenoDXInstallDialogAsync(IReadOnlyList<RenoDXTagInfo> nightlies);
}
