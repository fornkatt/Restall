// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Microsoft.Extensions.Logging.Abstractions;
using Restall.Application.Common.Enums;
using Restall.Infrastructure.Services;
using Restall.Infrastructure.Tests.Fakes;
using System.IO.Pipelines;
using System.Net;

namespace Restall.Infrastructure.Tests.ServiceTests;

public class DownloadServiceTests : IDisposable
{
    private const string AddonUrl = "https://restalltests.com/renodx-game.addon64";
    private static readonly byte[] s_addonBytes = [1, 2, 3, 4, 5];
    private readonly string _tempDirectory = Directory.CreateTempSubdirectory("restall-download-tests").FullName;
    private readonly string _destinationPath;

    public DownloadServiceTests()
    {
        _destinationPath = Path.Combine(_tempDirectory, "renodx-game.addon64");
    }

    public void Dispose()
    {
        Directory.Delete(_tempDirectory, recursive: true);
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task DownloadAsync_FileAvailable_ReturnsSuccessAndWritesFile()
    {
        var handler = new FakeHttpMessageHandler();
        handler.RespondWith(AddonUrl, FileResponse(s_addonBytes));
        var sut = CreateService(handler);

        var actual = await sut.DownloadAsync(new Uri(AddonUrl), _destinationPath, cancellationToken:
            TestContext.Current.CancellationToken);

        Assert.True(actual.IsSuccess);
        Assert.Equal(s_addonBytes,
            await File.ReadAllBytesAsync(_destinationPath, TestContext.Current.CancellationToken));
        Assert.Empty(Directory.GetFiles(_tempDirectory, "*.part"));
    }

    [Fact]
    public async Task DownloadAsync_DestinationAlreadyExists_ReplacesFile()
    {
        await File.WriteAllTextAsync(_destinationPath, "old file", TestContext.Current.CancellationToken);
        var handler = new FakeHttpMessageHandler();
        handler.RespondWith(AddonUrl, FileResponse(s_addonBytes));
        var sut = CreateService(handler);

        var actual = await sut.DownloadAsync(new Uri(AddonUrl), _destinationPath, cancellationToken:
            TestContext.Current.CancellationToken);

        Assert.True(actual.IsSuccess);
        Assert.Equal(s_addonBytes,
            await File.ReadAllBytesAsync(_destinationPath, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task DownloadAsync_FileNotFound_ReturnsDownloadFailedAndKeepsExistingFile()
    {
        await File.WriteAllTextAsync(_destinationPath, "old file", TestContext.Current.CancellationToken);
        var handler = new FakeHttpMessageHandler();
        handler.RespondWith(AddonUrl, new HttpResponseMessage(HttpStatusCode.NotFound));
        var sut = CreateService(handler);

        var actual = await sut.DownloadAsync(new Uri(AddonUrl), _destinationPath, cancellationToken:
            TestContext.Current.CancellationToken);

        Assert.False(actual.IsSuccess);
        Assert.Equal(ErrorType.DownloadFailed, actual.ErrorType);
        Assert.Equal("old file", await File.ReadAllTextAsync(_destinationPath, TestContext.Current.CancellationToken));
        Assert.Empty(Directory.GetFiles(_tempDirectory, "*.part"));
    }

    [Fact]
    public async Task DownloadAsync_DownloadStalls_ReturnsNetworkTimeoutAndLeavesNoPartFile()
    {
        var handler = new FakeHttpMessageHandler();
        handler.RespondWith(AddonUrl, await StallingResponseAsync());
        var sut = CreateService(handler, TimeSpan.FromMilliseconds(200));

        var actual = await sut.DownloadAsync(new Uri(AddonUrl), _destinationPath, cancellationToken:
            TestContext.Current.CancellationToken);

        Assert.False(actual.IsSuccess);
        Assert.Equal(ErrorType.NetworkTimeout, actual.ErrorType);
        Assert.False(File.Exists(_destinationPath));
        Assert.Empty(Directory.GetFiles(_tempDirectory, "*.part"));
    }

    [Fact]
    public async Task DownloadAsync_CancelledByCaller_ThrowsOperationCanceledException()
    {
        var handler = new FakeHttpMessageHandler();
        handler.RespondWith(AddonUrl, await StallingResponseAsync());
        var sut = CreateService(handler);
        using var cancellation = new CancellationTokenSource(TimeSpan.FromMilliseconds(200));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            sut.DownloadAsync(new Uri(AddonUrl), _destinationPath, cancellationToken: cancellation.Token));
        Assert.Empty(Directory.GetFiles(_tempDirectory, "*.part"));
    }

    private static DownloadService CreateService(FakeHttpMessageHandler handler, TimeSpan? stallTimeout = null) =>
        new(NullLogger<DownloadService>.Instance, new FakeHttpClientFactory(handler), pathService: null!)
        {
            StallTimeout = stallTimeout ?? TimeSpan.FromSeconds(3)
        };

    private static HttpResponseMessage FileResponse(byte[] bytes) =>
        new(HttpStatusCode.OK) { Content = new ByteArrayContent(bytes) };

    private static async Task<HttpResponseMessage> StallingResponseAsync()
    {
        var pipe = new Pipe();
        await pipe.Writer.WriteAsync(new byte[] { 1 });
        return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StreamContent(pipe.Reader.AsStream()) };
    }
}
