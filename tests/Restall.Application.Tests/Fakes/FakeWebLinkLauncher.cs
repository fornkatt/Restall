// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Application.Common;
using Restall.Application.Common.Enums;
using Restall.Application.Interfaces.Driven;

namespace Restall.Application.Tests.Fakes;

public sealed class FakeWebLinkLauncher : IWebLinkLauncher
{
    private readonly List<Uri> _openedUrls = [];
    private Result _result = Result.Success();

    public IReadOnlyList<Uri> OpenedUrls => _openedUrls;

    public void ReturnFailure(ErrorType errorType) =>
        _result = Result.Error("Fake shell launcher failure", errorType);

    public Result OpenWebLink(Uri url)
    {
        _openedUrls.Add(url);
        return _result;
    }
}
