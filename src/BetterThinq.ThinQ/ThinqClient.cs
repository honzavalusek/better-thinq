using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using BetterThinq.ThinQ.Models;
using Microsoft.Extensions.Logging;

namespace BetterThinq.ThinQ;

public sealed class ThinqClient(HttpClient http, ILogger<ThinqClient> logger) : IThinqClient
{
    public async Task<IReadOnlyList<DeviceSummary>> GetDevicesAsync(CancellationToken ct = default)
    {
        var payload = await GetJsonAsync("devices", ct);
        var data = Unwrap(payload);
        if (data is not JsonArray array)
        {
            logger.LogWarning("GET /devices returned unexpected shape: {Body}", payload.ToJsonString());
            return [];
        }

        var devices = new List<DeviceSummary>(array.Count);
        foreach (var node in array)
        {
            if (node is not JsonObject obj) continue;
            var deviceId = obj["deviceId"]?.GetValue<string>();
            if (string.IsNullOrEmpty(deviceId)) continue;
            var info = obj["deviceInfo"] as JsonObject;
            devices.Add(new DeviceSummary(
                DeviceId: deviceId,
                Alias: info?["alias"]?.GetValue<string>() ?? deviceId,
                ModelName: info?["modelName"]?.GetValue<string>() ?? "",
                DeviceType: info?["deviceType"]?.GetValue<string>() ?? ""));
        }
        return devices;
    }

    public async Task<DeviceState> GetDeviceAsync(string deviceId, CancellationToken ct = default)
    {
        var payload = await GetJsonAsync($"devices/{Uri.EscapeDataString(deviceId)}/state", ct);
        return new DeviceState(Unwrap(payload));
    }

    public async Task<DeviceProfile> GetProfileAsync(string deviceId, CancellationToken ct = default)
    {
        var payload = await GetJsonAsync($"devices/{Uri.EscapeDataString(deviceId)}/profile", ct);
        return new DeviceProfile(Unwrap(payload));
    }

    public async Task SendCommandAsync(string deviceId, JsonObject command, CancellationToken ct = default)
    {
        var url = $"devices/{Uri.EscapeDataString(deviceId)}/control";
        logger.LogInformation("POST {Url} body={Body}", url, command.ToJsonString());

        var req = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = JsonContent.Create((JsonNode)command),
        };
        req.Headers.TryAddWithoutValidation("x-conditional-control", "true");

        var resp = await http.SendAsync(req, ct);
        if (!resp.IsSuccessStatusCode)
        {
            var body = await resp.Content.ReadAsStringAsync(ct);
            throw new ThinqException($"ThinQ control failed: {(int)resp.StatusCode} {resp.ReasonPhrase}", resp.StatusCode, body);
        }
    }

    private async Task<JsonNode> GetJsonAsync(string url, CancellationToken ct)
    {
        var resp = await http.GetAsync(url, ct);
        var body = await resp.Content.ReadAsStringAsync(ct);
        if (!resp.IsSuccessStatusCode)
            throw new ThinqException($"ThinQ GET {url} failed: {(int)resp.StatusCode} {resp.ReasonPhrase}", resp.StatusCode, body);
        try
        {
            return JsonNode.Parse(body) ?? throw new ThinqException("ThinQ returned empty JSON", resp.StatusCode, body);
        }
        catch (JsonException ex)
        {
            throw new ThinqException($"ThinQ GET {url} returned non-JSON body", resp.StatusCode, body, ex);
        }
    }

    private static JsonNode Unwrap(JsonNode root)
    {
        if (root is JsonObject obj)
        {
            if (obj["response"] is { } r) return r;
            if (obj["data"] is { } d) return d;
        }
        return root;
    }
}
