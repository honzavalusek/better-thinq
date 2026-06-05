using System.Net.Http.Headers;
using Microsoft.Extensions.Options;

namespace BetterThinq.ThinQ;

public sealed class ThinqHeadersHandler(IOptionsMonitor<ThinqClientOptions> options) : DelegatingHandler
{
    // Public SDK API key shipped with LG's pythinqconnect (const.py: API_KEY).
    private const string ApiKey = "v6GFvkweNo7DK7yD3ylIZ9w52aKBU0eJ7wLXkSR3";

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var opts = options.CurrentValue;

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", opts.Pat);
        request.Headers.TryAddWithoutValidation("x-country", opts.Country);
        request.Headers.TryAddWithoutValidation("x-client-id", opts.ClientId);
        request.Headers.TryAddWithoutValidation("x-message-id", NewMessageId());
        request.Headers.TryAddWithoutValidation("x-api-key", ApiKey);
        request.Headers.TryAddWithoutValidation("x-service-phase", "OP");

        return base.SendAsync(request, cancellationToken);
    }

    // Matches pythinqconnect: base64-urlsafe(uuid.bytes)[:-2]
    private static string NewMessageId()
    {
        var bytes = Guid.NewGuid().ToByteArray();
        var b64 = Convert.ToBase64String(bytes).Replace('+', '-').Replace('/', '_');
        return b64[..^2].TrimEnd('=');
    }
}
