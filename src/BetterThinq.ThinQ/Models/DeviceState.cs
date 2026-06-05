using System.Text.Json.Nodes;

namespace BetterThinq.ThinQ.Models;

public sealed record DeviceState(JsonNode Raw)
{
    public string? StringAt(string dottedPath) => TryGetNode(dottedPath)?.GetValue<string>();

    public double? NumberAt(string dottedPath)
    {
        var node = TryGetNode(dottedPath);
        if (node is null) return null;
        try { return node.GetValue<double>(); }
        catch { return null; }
    }

    private JsonNode? TryGetNode(string dottedPath)
    {
        JsonNode? current = Raw;
        foreach (var part in dottedPath.Split('.'))
        {
            if (current is JsonObject obj && obj.TryGetPropertyValue(part, out var next))
                current = next;
            else
                return null;
        }
        return current;
    }
}
