using System.Net;
using Microsoft.Extensions.Options;

namespace BetterThinq.ThinQ.Tests;

public class HeadersHandlerTests
{
    [Fact]
    public async Task Adds_required_headers_per_request()
    {
        var fake = FakeHttpHandler.Json("{}");
        var headers = new ThinqHeadersHandler(StaticOptions(new ThinqClientOptions
        {
            Pat = "secret-pat",
            Country = "GB",
            ClientId = "client-guid-x",
        }))
        { InnerHandler = fake };

        var client = new HttpClient(headers) { BaseAddress = new Uri("https://api-eic.lgthinq.com/") };
        await client.GetAsync("devices");

        var req = fake.Requests.Single();
        Assert.Equal("Bearer", req.Headers.Authorization?.Scheme);
        Assert.Equal("secret-pat", req.Headers.Authorization?.Parameter);
        Assert.Equal("GB", req.Headers.GetValues("x-country").Single());
        Assert.Equal("client-guid-x", req.Headers.GetValues("x-client-id").Single());
        Assert.Equal("OP", req.Headers.GetValues("x-service-phase").Single());
        Assert.NotEmpty(req.Headers.GetValues("x-message-id").Single());
    }

    [Fact]
    public async Task Generates_new_message_id_per_request()
    {
        var fake = FakeHttpHandler.Json("{}");
        var headers = new ThinqHeadersHandler(StaticOptions(new ThinqClientOptions { Pat = "p", Country = "GB" }))
        { InnerHandler = fake };
        var client = new HttpClient(headers) { BaseAddress = new Uri("https://api-eic.lgthinq.com/") };

        await client.GetAsync("devices");
        await client.GetAsync("devices");

        var ids = fake.Requests.Select(r => r.Headers.GetValues("x-message-id").Single()).ToList();
        Assert.Equal(2, ids.Distinct().Count());
    }

    [Fact]
    public async Task X_api_key_header_is_set()
    {
        // pythinqconnect ships a public SDK key in const.py — required even for PAT auth.
        var fake = FakeHttpHandler.Json("{}");
        var headers = new ThinqHeadersHandler(StaticOptions(new ThinqClientOptions { Pat = "p", Country = "GB" }))
        { InnerHandler = fake };
        using var client = new HttpClient(headers) { BaseAddress = new Uri("https://api-eic.lgthinq.com/") };
        await client.GetAsync("devices");

        var key = fake.Requests.Single().Headers.GetValues("x-api-key").Single();
        Assert.NotEmpty(key);
    }

    private static IOptionsMonitor<ThinqClientOptions> StaticOptions(ThinqClientOptions opts) =>
        new StaticOptionsMonitor<ThinqClientOptions>(opts);
}

internal sealed class StaticOptionsMonitor<T>(T value) : IOptionsMonitor<T>
{
    public T CurrentValue { get; } = value;
    public T Get(string? name) => CurrentValue;
    public IDisposable? OnChange(Action<T, string?> listener) => null;
}
