namespace BetterThinq.ThinQ.Models;

public sealed record DeviceSummary(
    string DeviceId,
    string Alias,
    string ModelName,
    string DeviceType);
