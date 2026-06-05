using BetterThinq.ThinQ;

namespace BetterThinq.Web.Domain;

public class Settings
{
    public int Id { get; set; } = 1;
    public string EncryptedPat { get; set; } = "";
    public string Country { get; set; } = "GB";
    public ThinqRegion Region { get; set; } = ThinqRegion.Eic;
    public string ClientId { get; set; } = Guid.NewGuid().ToString();
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
}
