using System.Net;
using System.Text;

namespace Restall.Tests.TestUtilities;

internal sealed class FakeHttpClientFactory : IHttpClientFactory
{
    private readonly HttpClient _client;

    public FakeHttpClientFactory(HttpClient client)
    {
        _client = client;
    }

    public HttpClient CreateClient(string name) => _client;
}

internal sealed class FakeHttpMessageHandler : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, HttpResponseMessage> _handler;
    private readonly List<Uri?> _requestUris = [];
    private readonly object _lock = new();

    public FakeHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handler)
    {
        _handler = handler;
    }

    public IReadOnlyList<Uri?> RequestUris
    {
        get
        {
            lock (_lock)
            {
                return _requestUris.ToArray();
            }
        }
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        lock (_lock)
        {
            _requestUris.Add(request.RequestUri);
        }

        return Task.FromResult(_handler(request));
    }

    public static HttpResponseMessage TextResponse(string content, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        return new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(content, Encoding.UTF8)
        };
    }

    public static HttpResponseMessage BytesResponse(byte[] content, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        return new HttpResponseMessage(statusCode)
        {
            Content = new ByteArrayContent(content)
        };
    }
}
