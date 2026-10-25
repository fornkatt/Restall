// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Application.DTOs.Responses;
using Restall.Application.Interfaces.Driving;

namespace Restall.UI.Tests.Fakes;

public sealed class FakeOpenWebLinkUseCase : IOpenWebLinkUseCase
{
    private readonly List<Uri> _openedUrls = [];
    private OpenWebLinkResponse _response = new(true);

    public IReadOnlyList<Uri> OpenedUrls => _openedUrls;

    public void ReturnFailure(string message) => _response = new OpenWebLinkResponse(false, message);

    public void ReturnSuccess() => _response = new OpenWebLinkResponse(true);

    public OpenWebLinkResponse Execute(Uri url)
    {
        _openedUrls.Add(url);

        return _response;
    }
}
