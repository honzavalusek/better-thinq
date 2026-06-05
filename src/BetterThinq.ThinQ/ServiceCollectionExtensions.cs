using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace BetterThinq.ThinQ;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddThinqClient(this IServiceCollection services)
    {
        services.AddTransient<ThinqHeadersHandler>();
        services.AddTransient<ThinqRetryHandler>();

        services.AddHttpClient<IThinqClient, ThinqClient>((sp, http) =>
        {
            var opts = sp.GetRequiredService<IOptionsMonitor<ThinqClientOptions>>().CurrentValue;
            http.BaseAddress = opts.Region.BaseUri();
            http.Timeout = TimeSpan.FromSeconds(30);
            http.DefaultRequestHeaders.Accept.Add(new("application/json"));
        })
        .AddHttpMessageHandler<ThinqHeadersHandler>()
        .AddHttpMessageHandler<ThinqRetryHandler>();

        return services;
    }
}
