using System.Text.Json.Nodes;

namespace BetterThinq.ThinQ.Models;

public sealed record DeviceProfile(JsonNode Raw)
{
    public bool HasKey(string key) => ContainsKey(Raw, key);

    private static bool ContainsKey(JsonNode? node, string key)
    {
        switch (node)
        {
            case JsonObject obj:
                if (obj.ContainsKey(key)) return true;
                foreach (var kv in obj)
                    if (ContainsKey(kv.Value, key))
                        return true;
                return false;
            case JsonArray arr:
                foreach (var item in arr)
                    if (ContainsKey(item, key))
                        return true;
                return false;
            default:
                return false;
        }
    }
}
