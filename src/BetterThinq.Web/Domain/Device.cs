namespace BetterThinq.Web.Domain;

public class Device
{
    public string Id { get; set; } = "";
    public string Alias { get; set; } = "";
    public string ModelName { get; set; } = "";
    public string DeviceType { get; set; } = "";
    public string ProfileJson { get; set; } = "{}";
    public DateTime LastSeenUtc { get; set; }
}
