// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Restall.Infrastructure.Tests.Fakes;

internal sealed class FakeHttpMessageHandler : HttpMessageHandler
{
    private readonly IReadOnlyDictionary<string, Func<HttpResponseMessage>> _responses;

    public FakeHttpMessageHandler(IReadOnlyDictionary<string, Func<HttpResponseMessage>> responses) =>
        _responses = responses;

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var url = request.RequestUri?.AbsoluteUri
                  ?? throw new InvalidOperationException("The rquest has no URL");

        if (!_responses.TryGetValue(url, out var respond))
            throw new InvalidOperationException($"No fake response is set up for \"{url}\"");

        return Task.FromResult(respond());
    }
}
