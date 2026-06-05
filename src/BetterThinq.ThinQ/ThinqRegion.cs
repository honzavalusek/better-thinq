namespace BetterThinq.ThinQ;

public enum ThinqRegion
{
    Eic,
    Aic,
    Kic,
}

public static class ThinqRegionExtensions
{
    public static Uri BaseUri(this ThinqRegion region) => region switch
    {
        ThinqRegion.Eic => new Uri("https://api-eic.lgthinq.com/"),
        ThinqRegion.Aic => new Uri("https://api-aic.lgthinq.com/"),
        ThinqRegion.Kic => new Uri("https://api-kic.lgthinq.com/"),
        _ => throw new ArgumentOutOfRangeException(nameof(region), region, "Unknown ThinQ region"),
    };
}
