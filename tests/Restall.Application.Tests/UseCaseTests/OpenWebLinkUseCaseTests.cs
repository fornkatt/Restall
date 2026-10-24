// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Microsoft.Extensions.Logging.Abstractions;
using Restall.Application.Common.Enums;
using Restall.Application.Tests.Fakes;
using Restall.Application.UseCases;

namespace Restall.Application.Tests.UseCaseTests;

public class OpenWebLinkUseCaseTests
{
    private readonly FakeWebLinkLauncher _webLinkLauncher = new();

    [Fact]
    public void Execute_HttpsLink_ReturnsSuccessAndOpensLink()
    {
        var url = new Uri("https://www.nexusmods.com/games/mods/1");
        var sut = CreateUseCase();
        var actual = sut.Execute(url);

        Assert.True(actual.IsSuccess);
        Assert.Null(actual.Message);
        Assert.Equal(url, Assert.Single(_webLinkLauncher.OpenedUrls));
    }

    [Theory]
    [InlineData("http://www.nexusmods.com/games/mods/1")]
    [InlineData("file:///home/user/evil.sh")]
    [InlineData("file://attacker/share/evil.exe")]
    [InlineData("steam://run/440")]
    [InlineData("ms-msdt:/id PCWDiagnostic")]
    [InlineData("javascript:alert(1)")]
    [InlineData("smb://attacker/share")]
    [InlineData("mailto:someone@example.com")]
    [InlineData("relative/path")]
    [InlineData("https://www.nexusmods.com@evil.example/mods/1")]
    [InlineData("https://user:password@example.com/")]
    public void Execute_NotAPlainHttpsLink_ReturnsRefusalAndDoesNotOpenLink(string url)
    {
        var sut = CreateUseCase();
        var actual = sut.Execute(new Uri(url, UriKind.RelativeOrAbsolute));

        Assert.False(actual.IsSuccess);
        Assert.Equal("This link isn't a plain https address, so Restall won't open it.", actual.Message);
        Assert.Empty(_webLinkLauncher.OpenedUrls);
    }

    [Fact]
    public void Execute_LauncherFails_ReturnsFailureMessageWithHost()
    {
        _webLinkLauncher.ReturnFailure(ErrorType.ProcessStartFailed);
        var sut = CreateUseCase();
        var actual = sut.Execute(new Uri("https://www.nexusmods.com/games/mods/1"));

        Assert.False(actual.IsSuccess);
        Assert.Equal("Couldn't open www.nexusmods.com. No app is set up to open web links, or it failed to start.",
            actual.Message);
    }

    private OpenWebLinkUseCase CreateUseCase() =>
        new(NullLogger<OpenWebLinkUseCase>.Instance, _webLinkLauncher);
}
