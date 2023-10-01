using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;

namespace TestHttpClientSample;

public class HttpClientMock : IAsyncDisposable
{
    private bool _running;

    public HttpClientMock()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        Application = builder.Build();
    }

    public WebApplication Application { get; }

    public HttpClient CreateHttpClient()
    {
        StartServer();
        return Application.GetTestClient();
    }

    private void StartServer()
    {
        if (_running) return;
        _running = true;
        _ = Application.RunAsync();
    }

    public async ValueTask DisposeAsync() => await Application.DisposeAsync();
}