namespace BetterThinq.ThinQ.Tests;

public class RegionTests
{
    [Theory]
    [InlineData(ThinqRegion.Eic, "https://api-eic.lgthinq.com/")]
    [InlineData(ThinqRegion.Aic, "https://api-aic.lgthinq.com/")]
    [InlineData(ThinqRegion.Kic, "https://api-kic.lgthinq.com/")]
    public void Maps_region_to_base_uri(ThinqRegion region, string expected)
    {
        Assert.Equal(new Uri(expected), region.BaseUri());
    }
}
