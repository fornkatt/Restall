// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Application.Common;
using Restall.Application.DTOs.RenoDXDTOs;
using Restall.Application.Interfaces.Driven;
using System.Collections.Immutable;

namespace Restall.Application.Tests.Fakes;

public sealed class FakeRenoDXModSourceService : IRenoDXModSourceService
{
    private Result<ImmutableArray<RenoDXGameMod>>? _gameMods;
    private Result<ImmutableArray<RenoDXUnrealGenericMod>>? _unrealGenericMods;
    private Result<ImmutableArray<RenoDXUnityGenericMod>>? _unityGenericMods;

    public void ReturnGameMods(ImmutableArray<RenoDXGameMod> gameMods) =>
        _gameMods = Result<ImmutableArray<RenoDXGameMod>>.Success(gameMods);

    public void ReturnGameModsFailure() =>
        _gameMods = Result<ImmutableArray<RenoDXGameMod>>.Error("Fake game mods failure");

    public void ReturnUnrealGenericMods(ImmutableArray<RenoDXUnrealGenericMod> unrealGenericMods) =>
        _unrealGenericMods = Result<ImmutableArray<RenoDXUnrealGenericMod>>.Success(unrealGenericMods);

    public void ReturnUnityGenericMods(ImmutableArray<RenoDXUnityGenericMod> unityGenericMods) =>
        _unityGenericMods = Result<ImmutableArray<RenoDXUnityGenericMod>>.Success(unityGenericMods);

    public Task<Result<ImmutableArray<RenoDXGameMod>>> FetchGameModsAsync(
        CancellationToken cancellationToken = default) =>
        Task.FromResult(_gameMods ?? throw new InvalidOperationException("No fake RenoDX game mods are set up"));

    public Task<Result<ImmutableArray<RenoDXUnrealGenericMod>>> FetchUnrealGenericModsAsync(
        CancellationToken cancellationToken = default) =>
        Task.FromResult(_unrealGenericMods ??
                        throw new InvalidOperationException("No fake RenoDX Unreal generic mods are set up"));

    public Task<Result<ImmutableArray<RenoDXUnityGenericMod>>> FetchUnityGenericModsAsync(
        CancellationToken cancellationToken = default) =>
        Task.FromResult(_unityGenericMods ??
                        throw new InvalidOperationException("No fake RenoDX Unity generic mods are set up"));
}
