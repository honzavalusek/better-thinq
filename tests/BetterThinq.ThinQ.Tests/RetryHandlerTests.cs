using System.Net;

namespace BetterThinq.ThinQ.Tests;

public class RetryHandlerTests
{
    [Fact]
    public async Task Retries_on_429_then_succeeds()
    {
        var responses = new[]
        {
            new HttpResponseMessage(HttpStatusCode.TooManyRequests) { Content = new StringContent("slow down") },
            new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("ok") },
        };
        var fake = FakeHttpHandler.Sequence(responses);
        var retry = new ThinqRetryHandler { InnerHandler = fake };
        var http = new HttpClient(retry) { BaseAddress = new Uri("https://example/") };

        var resp = await http.GetAsync("x");

        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        Assert.Equal(2, fake.Requests.Count);
    }

    [Fact]
    public async Task Does_not_retry_on_400()
    {
        var fake = FakeHttpHandler.Json("nope", HttpStatusCode.BadRequest);
        var retry = new ThinqRetryHandler { InnerHandler = fake };
        var http = new HttpClient(retry) { BaseAddress = new Uri("https://example/") };

        var resp = await http.GetAsync("x");

        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        Assert.Single(fake.Requests);
    }
}
