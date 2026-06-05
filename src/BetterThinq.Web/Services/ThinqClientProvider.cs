using BetterThinq.ThinQ;
using BetterThinq.Web.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BetterThinq.Web.Services;

public sealed class ThinqClientProvider(
    IServiceScopeFactory scopeFactory,
    PatProtector protector,
    ILoggerFactory loggerFactory) : IDisposable
{
    private readonly Lock _lock = new();
    private HttpClient? _http;
    private IThinqClient? _client;

    public async Task<IThinqClient?> GetAsync(CancellationToken ct = default)
    {
        if (_client is not null) return _client;

        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var settings = await db.Settings.AsNoTracking().FirstOrDefaultAsync(ct);
        if (settings is null) return null;

        lock (_lock)
        {
            if (_client is not null) return _client;
            var pat = protector.Decrypt(settings.EncryptedPat);
            var options = new StaticOptionsMonitor(new ThinqClientOptions
            {
                Pat = pat,
                Region = settings.Region,
                Country = settings.Country,
                ClientId = settings.ClientId,
            });
            var headers = new ThinqHeadersHandler(options) { InnerHandler = new SocketsHttpHandler() };
            var retry = new ThinqRetryHandler { InnerHandler = headers };
            _http = new HttpClient(retry)
            {
                BaseAddress = settings.Region.BaseUri(),
                Timeout = TimeSpan.FromSeconds(30),
            };
            _client = new ThinqClient(_http, loggerFactory.CreateLogger<ThinqClient>());
            return _client;
        }
    }

    public void Invalidate()
    {
        lock (_lock)
        {
            _http?.Dispose();
            _http = null;
            _client = null;
        }
    }

    public void Dispose() => Invalidate();

    private sealed class StaticOptionsMonitor(ThinqClientOptions value) : IOptionsMonitor<ThinqClientOptions>
    {
        public ThinqClientOptions CurrentValue { get; } = value;
        public ThinqClientOptions Get(string? name) => CurrentValue;
        public IDisposable? OnChange(Action<ThinqClientOptions, string?> listener) => null;
    }
}
