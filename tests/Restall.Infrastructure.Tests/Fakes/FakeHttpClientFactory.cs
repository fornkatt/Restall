// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Restall.Infrastructure.Tests.Fakes;

public class FakeHttpClientFactory : IHttpClientFactory
{
    private readonly HttpMessageHandler _handler;

    public FakeHttpClientFactory(HttpMessageHandler handler) =>
        _handler = handler;

    public HttpClient CreateClient(string name) =>
        new(_handler, disposeHandler: false);
}
