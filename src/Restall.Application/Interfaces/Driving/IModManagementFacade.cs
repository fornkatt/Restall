// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Application.DTOs;
using Restall.Application.DTOs.Requests;
using Restall.Application.DTOs.Responses;
using Restall.Application.UseCases.Requests;
using Restall.Domain.Entities;

namespace Restall.Application.Interfaces.Driving;

public interface IModManagementFacade
{
    Task<ModOperationResponse> InstallOrUpdateReShadeAsync(InstallReShadeRequest request,
        IProgress<DownloadProgressReport>? progress = null);
    Task<ModOperationResponse> UninstallReShadeAsync(Game game);

    Task<RenoDXInstallResponse> InstallOrUpdateRenoDXAsync(RenoDXInstallRequest request,
        IProgress<DownloadProgressReport>? progress = null);
    Task<RenoDXUninstallResponse> UninstallRenoDXAsync(Game game);
}
