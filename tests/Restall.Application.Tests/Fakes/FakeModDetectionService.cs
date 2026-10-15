// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Application.Common;
using Restall.Application.Interfaces.Driven;
using Restall.Domain.Entities;

namespace Restall.Application.Tests.Fakes;

public sealed class FakeModDetectionService : IModDetectionService
{
    public Task<Result<HashSet<ReShade>>> DetectInstalledReShadeAsync(string executableDirectory) =>
        Task.FromResult(Result<HashSet<ReShade>>.Success([]));

    public Task<Result<HashSet<RenoDX>>> DetectInstalledRenoDXAsync(string executableDirectory) =>
        Task.FromResult(Result<HashSet<RenoDX>>.Success([]));

    public Result<string?> GetRenoDXFileVersion(string filePath) =>
        throw new NotSupportedException("The library refresh doesn't read RenoDX file versions");
}
