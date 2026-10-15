// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Application.Common;
using Restall.Application.Common.Enums;
using Restall.Application.DTOs.RenoDXDTOs;
using Restall.Application.Interfaces.Driven;
using System.Collections.Immutable;

namespace Restall.Application.Tests.Fakes;

public sealed class FakeParseService : IParseService
{
    private ImmutableArray<string>? _reShadeVersions;
    private Result<RenoDXTagInfo>? _snapshot;
    private Result<ImmutableArray<RenoDXTagInfo>>? _nightlies;

    public void ReturnReShadeVersions(ImmutableArray<string> reShadeVersions) => _reShadeVersions = reShadeVersions;

    public void ReturnSnapshot(RenoDXTagInfo snapshot) => _snapshot = Result<RenoDXTagInfo>.Success(snapshot);

    public void ReturnPartialSnapshot(RenoDXTagInfo snapshot, WarningType warningType) =>
        _snapshot = Result<RenoDXTagInfo>.Partial(snapshot, warningType: warningType);

    public void ReturnSnapshotFailure() => _snapshot = Result<RenoDXTagInfo>.Error("Fake Snapshot failure");

    public void ReturnNightlies(ImmutableArray<RenoDXTagInfo> nightlies) =>
        _nightlies = Result<ImmutableArray<RenoDXTagInfo>>.Success(nightlies);

    public Task<ImmutableArray<string>> FetchReShadeVersionsAsync() =>
        Task.FromResult(_reShadeVersions ?? throw new InvalidOperationException("No fake ReShade versions are set up"));

    public Task<Result<RenoDXTagInfo>> FetchRenoDXSnapshotAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(_snapshot ?? throw new InvalidOperationException("No fake snapshot is set up"));

    public Task<Result<ImmutableArray<RenoDXTagInfo>>> FetchRenoDXNightlyTagsAsync(
        CancellationToken cancellationToken = default) =>
        Task.FromResult(_nightlies ?? throw new InvalidOperationException("No fake Nightlies are set up"));
}
