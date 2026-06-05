using System.Net;

namespace BetterThinq.ThinQ;

public sealed class ThinqRetryHandler : DelegatingHandler
{
    private const int MaxAttempts = 4;
    private static readonly TimeSpan[] Backoffs = [
        TimeSpan.FromSeconds(1),
        TimeSpan.FromSeconds(2),
        TimeSpan.FromSeconds(4),
    ];

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        HttpResponseMessage? response = null;
        for (var attempt = 0; attempt < MaxAttempts; attempt++)
        {
            response = await base.SendAsync(request, cancellationToken);

            if (!ShouldRetry(response.StatusCode) || attempt == MaxAttempts - 1)
                return response;

            var wait = response.Headers.RetryAfter?.Delta ?? Backoffs[attempt];
            wait += TimeSpan.FromMilliseconds(Random.Shared.Next(0, 250));
            response.Dispose();
            await Task.Delay(wait, cancellationToken);
        }
        return response!;
    }

    private static bool ShouldRetry(HttpStatusCode code) =>
        code == HttpStatusCode.TooManyRequests || (int)code >= 500;
}
