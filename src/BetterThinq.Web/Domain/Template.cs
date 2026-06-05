namespace BetterThinq.Web.Domain;

public class Template
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "";
    public AcMode? Mode { get; set; }
    public int? TargetTemperatureC { get; set; }
    public AcFanSpeed? FanSpeed { get; set; }
}
