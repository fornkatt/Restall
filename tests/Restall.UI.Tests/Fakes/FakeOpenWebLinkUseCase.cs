// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Application.DTOs.Responses;
using Restall.Application.Interfaces.Driving;

namespace Restall.UI.Tests.Fakes;

public class FakeOpenWebLinkUseCase : IOpenWebLinkUseCase
{
    private readonly List<Uri> _openedUrls = [];

    public IReadOnlyList<Uri> OpenedUrls => _openedUrls;

    public OpenWebLinkResponse Execute(Uri url)
    {
        _openedUrls.Add(url);

        return new OpenWebLinkResponse(true);
    }
}
