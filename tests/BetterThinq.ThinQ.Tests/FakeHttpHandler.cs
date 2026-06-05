using System.Net;

namespace BetterThinq.ThinQ.Tests;

internal sealed class FakeHttpHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
{
    public List<HttpRequestMessage> Requests { get; } = [];

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Requests.Add(request);
        return Task.FromResult(respond(request));
    }

    public static FakeHttpHandler Json(string body, HttpStatusCode status = HttpStatusCode.OK) =>
        new(_ => new HttpResponseMessage(status)
        {
            Content = new StringContent(body, System.Text.Encoding.UTF8, "application/json"),
        });

    public static FakeHttpHandler Sequence(params HttpResponseMessage[] responses)
    {
        var i = 0;
        return new FakeHttpHandler(_ => responses[Math.Min(i++, responses.Length - 1)]);
    }
}
