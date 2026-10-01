// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Microsoft.Extensions.Logging;
using Restall.Application.Common;
using Restall.Application.DTOs.RenoDXDTOs;
using Restall.Application.Interfaces.Driven;
using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Restall.Infrastructure.Services;

internal sealed partial class RenoDXModSourceService : IRenoDXModSourceService
{
    internal const string HttpClientName = nameof(RenoDXModSourceService);
    private const string DatabaseUrl = "https://raw.githubusercontent.com/RankFTW/rhi-repo/main/database/";
    internal const string GameModsUrl = DatabaseUrl + "RenoDXdb.json";
    internal const string UnrealGenericModsUrl = DatabaseUrl + "RenoDXdb-unreal.json";
    internal const string UnityGenericModsUrl = DatabaseUrl + "RenoDXdb-unity.json";

    private static readonly JsonSerializerOptions s_jsonOptions = new(JsonSerializerDefaults.Web)
    {
        RespectNullableAnnotations = true,
        RespectRequiredConstructorParameters = true,
        Converters = { new JsonStringEnumConverter(namingPolicy: null, allowIntegerValues: false) }
    };

    private readonly ILogger<RenoDXModSourceService> _logger;
    private readonly IHttpClientFactory _clientFactory;

    public RenoDXModSourceService(
        ILogger<RenoDXModSourceService> logger,
        IHttpClientFactory clientFactory)
    {
        _logger = logger;
        _clientFactory = clientFactory;
    }

    public Task<Result<ImmutableArray<RenoDXGameMod>>> FetchGameModsAsync(
        CancellationToken cancellationToken = default) =>
        FetchEntriesAsync<RenoDXGameMod>(GameModsUrl, cancellationToken);

    public Task<Result<ImmutableArray<RenoDXUnrealGenericMod>>> FetchUnrealGenericModsAsync(
        CancellationToken cancellationToken = default) =>
        FetchEntriesAsync<RenoDXUnrealGenericMod>(UnrealGenericModsUrl, cancellationToken);

    public Task<Result<ImmutableArray<RenoDXUnityGenericMod>>> FetchUnityGenericModsAsync(
        CancellationToken cancellationToken = default) =>
        FetchEntriesAsync<RenoDXUnityGenericMod>(UnityGenericModsUrl, cancellationToken);

    private async Task<Result<ImmutableArray<T>>> FetchEntriesAsync<T>(string url,
        CancellationToken cancellationToken = default) where T : class
    {
        LogRenoDXDatabaseFileFetchStart(url);

        JsonElement[]? elements;

        try
        {
            var httpClient = _clientFactory.CreateClient(HttpClientName);
            var json = await httpClient.GetByteArrayAsync(url, cancellationToken);
            elements = JsonSerializer.Deserialize<JsonElement[]>(json);
        }
        catch (HttpRequestException ex)
        {
            return Result<ImmutableArray<T>>.Error($"Could not download 'RenoDX' mod database file from \"{url}\"",
                exception: ex);
        }
        catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            return Result<ImmutableArray<T>>.Error($"Request to \"{url}\" timed out", exception: ex);
        }
        catch (JsonException ex)
        {
            return Result<ImmutableArray<T>>.Error($"Could not parse 'RenoDX' mod database file \"{url}\" as a JSON " +
                                                   $"array", exception: ex);
        }

        if (elements is null)
        {
            return Result<ImmutableArray<T>>.Error($"Could not find a JSON array in 'RenoDX' mod database file" +
                                                   $" \"{url}\"");
        }

        var entries = ImmutableArray.CreateBuilder<T>(elements.Length);

        foreach (var element in elements)
        {
            var entry = ReadEntry<T>(url, element);

            if (entry is not null)
                entries.Add(entry);
        }

        var skippedCount = elements.Length - entries.Count;

        LogRenoDXDatabaseFileReadComplete(entries.Count, url, skippedCount);

        if (skippedCount > 0)
            return Result<ImmutableArray<T>>.Partial(entries.DrainToImmutable());

        return Result<ImmutableArray<T>>.Success(entries.DrainToImmutable());
    }

    private T? ReadEntry<T>(string url, JsonElement element) where T : class
    {
        T? entry;

        try
        {
            entry = element.Deserialize<T>(s_jsonOptions);
        }
        catch (JsonException ex)
        {
            LogRenoDXDatabaseFileEntryReadFailure(url, element.GetRawText(), ex);
            return null;
        }

        if (entry is null)
            LogRenoDXDatabaseFileEntryNotFound(url);

        return entry;
    }
}
