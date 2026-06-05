using BetterThinq.Web.Components;
using BetterThinq.Web.Middleware;
using BetterThinq.Web.Persistence;
using BetterThinq.Web.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;

namespace BetterThinq.Web;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var dataDir = Path.Combine(builder.Environment.ContentRootPath, "data");
        Directory.CreateDirectory(dataDir);
        Directory.CreateDirectory(Path.Combine(dataDir, "keys"));

        builder.Services.AddDbContext<AppDbContext>(opts =>
            opts.UseSqlite($"Data Source={Path.Combine(dataDir, "betterthinq.db")}"));

        builder.Services.AddDataProtection()
            .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(dataDir, "keys")))
            .SetApplicationName("BetterThinq");

        builder.Services.AddSingleton<PatProtector>();
        builder.Services.AddSingleton<ThinqClientProvider>();

        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.Migrate();
        }

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

        app.UseAntiforgery();

        app.UseMiddleware<RequireSetupMiddleware>();

        app.MapStaticAssets();
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        app.Run();
    }
}
