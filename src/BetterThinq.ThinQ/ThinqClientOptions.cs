namespace BetterThinq.ThinQ;

public sealed class ThinqClientOptions
{
    public string Pat { get; set; } = "";
    public ThinqRegion Region { get; set; } = ThinqRegion.Eic;
    public string Country { get; set; } = "GB";
    public string ClientId { get; set; } = Guid.NewGuid().ToString();
}
