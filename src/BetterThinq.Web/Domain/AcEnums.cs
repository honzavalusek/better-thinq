namespace BetterThinq.Web.Domain;

public enum AcPower
{
    On,
    Off,
}

public enum AcMode
{
    Cool,
    Heat,
    Dry,
    Fan,
    Auto,
}

public enum AcFanSpeed
{
    Low,
    Mid,
    High,
}

public static class AcEnumMappings
{
    public static string ToThinq(this AcPower power) => power switch
    {
        AcPower.On => "POWER_ON",
        AcPower.Off => "POWER_OFF",
        _ => throw new ArgumentOutOfRangeException(nameof(power)),
    };

    public static string ToThinq(this AcMode mode) => mode switch
    {
        AcMode.Cool => "COOL",
        AcMode.Heat => "HEAT",
        AcMode.Dry => "DRY",
        AcMode.Fan => "FAN",
        AcMode.Auto => "AUTO",
        _ => throw new ArgumentOutOfRangeException(nameof(mode)),
    };

    public static string ToThinq(this AcFanSpeed speed) => speed switch
    {
        AcFanSpeed.Low => "LOW",
        AcFanSpeed.Mid => "MID",
        AcFanSpeed.High => "HIGH",
        _ => throw new ArgumentOutOfRangeException(nameof(speed)),
    };

}
