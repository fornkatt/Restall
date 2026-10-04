// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Application.DTOs;
using Restall.Application.DTOs.Results;
using Restall.Application.UseCases.Requests;
using Restall.Domain.Entities;

namespace Restall.Application.Interfaces.Driving;

public interface IModManagementFacade
{
    Task<ModOperationResultDto> InstallOrUpdateReShadeAsync(InstallReShadeRequest request,
        IProgress<DownloadProgressReport>? progress = null);

    Task<ModOperationResultDto> InstallOrUpdateRenoDXAsync(InstallRenoDXRequest request,
        IProgress<DownloadProgressReport>? progress = null);

    Task<ModOperationResultDto> UninstallReShadeAsync(Game game);
    Task<ModOperationResultDto> UninstallRenoDXAsync(Game game);
}
