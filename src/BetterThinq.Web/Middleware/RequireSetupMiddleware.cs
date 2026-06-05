using BetterThinq.Web.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BetterThinq.Web.Middleware;

public sealed class RequireSetupMiddleware(RequestDelegate next)
{
    private static readonly string[] AllowedPathPrefixes =
        ["/setup", "/_framework", "/_blazor", "/css", "/js", "/favicon", "/lib", "/_content"];

    public async Task Invoke(HttpContext ctx, AppDbContext db)
    {
        var path = ctx.Request.Path.Value ?? "";
        if (AllowedPathPrefixes.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
        {
            await next(ctx);
            return;
        }

        var configured = await db.Settings.AsNoTracking().AnyAsync(ctx.RequestAborted);
        if (!configured)
        {
            ctx.Response.Redirect("/setup");
            return;
        }

        await next(ctx);
    }
}
