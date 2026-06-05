using System.Net;

namespace BetterThinq.ThinQ;

public sealed class ThinqException(string message, HttpStatusCode status, string? body, Exception? inner = null)
    : Exception(message, inner)
{
    public HttpStatusCode Status { get; } = status;
    public string? Body { get; } = body;
}
