using System.Text.Json.Nodes;
using BetterThinq.ThinQ.Models;

namespace BetterThinq.ThinQ;

public interface IThinqClient
{
    Task<IReadOnlyList<DeviceSummary>> GetDevicesAsync(CancellationToken ct = default);
    Task<DeviceState> GetDeviceAsync(string deviceId, CancellationToken ct = default);
    Task<DeviceProfile> GetProfileAsync(string deviceId, CancellationToken ct = default);
    Task SendCommandAsync(string deviceId, JsonObject command, CancellationToken ct = default);
}
