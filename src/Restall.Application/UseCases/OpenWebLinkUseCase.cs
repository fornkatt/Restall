// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Microsoft.Extensions.Logging;
using Restall.Application.Common;
using Restall.Application.Common.Enums;
using Restall.Application.DTOs.Responses;
using Restall.Application.Interfaces.Driven;
using Restall.Application.Interfaces.Driving;

namespace Restall.Application.UseCases;

public sealed partial class OpenWebLinkUseCase : IOpenWebLinkUseCase
{
    private const string NotAPlainHttpsLinkMessage = "This link isn't a plain https address, so Restall won't open it.";

    private readonly ILogger<OpenWebLinkUseCase> _logger;
    private readonly IWebLinkLauncher _webLinkLauncher;

    public OpenWebLinkUseCase(
        ILogger<OpenWebLinkUseCase> logger,
        IWebLinkLauncher webLinkLauncher
    )
    {
        _logger = logger;
        _webLinkLauncher = webLinkLauncher;
    }

    public OpenWebLinkResponse Execute(Uri url)
    {
        UIThreadGuard.AssertNotOnUIThread();

        if (!IsPlainHttpsLink(url))
        {
            LogLinkRefused(url.OriginalString);
            return new OpenWebLinkResponse(false, NotAPlainHttpsLinkMessage);
        }

        var result = _webLinkLauncher.OpenWebLink(url);

        if (result.IsSuccess)
            return new OpenWebLinkResponse(true);

        LogWebLinkOpenFailure(url.AbsoluteUri, result.Message, result.Exception);

        return new OpenWebLinkResponse(false, GetOpenFailureMessage(result.ErrorType, url));
    }

    private static bool IsPlainHttpsLink(Uri url) =>
        url is { IsAbsoluteUri: true, Scheme: "https", UserInfo.Length: 0 };

    private static string GetOpenFailureMessage(ErrorType errorType, Uri url) => errorType switch
    {
        ErrorType.ProcessStartFailed =>
            $"Couldn't open {url.Host}. No app is set up to open web links, or it failed to start.",
        _ => $"Couldn't open {url.Host}. Check the log for details."
    };
}
