using System.Text.Json.Nodes;
using BetterThinq.ThinQ.Models;
using BetterThinq.Web.Domain;
using BetterThinq.Web.ThinQ;

namespace BetterThinq.Web.Tests;

public class ThinqCommandBuilderTests
{
    private static DeviceProfile FullAcProfile()
    {
        var json = """
            {
              "property": {
                "operation": { "airConOperationMode": {"mode":["r","w"]} },
                "airConJobMode": { "currentJobMode": {"mode":["r","w"]} },
                "temperature": { "targetTemperature": {"mode":["r","w"]}, "unit": {"mode":["r"]} },
                "airFlow": { "windStrength": {"mode":["r","w"]} }
              }
            }
            """;
        return new DeviceProfile(JsonNode.Parse(json)!);
    }

    [Fact]
    public void Always_powers_on_first_then_emits_other_fields()
    {
        var template = new Template
        {
            Name = "Night",
            Mode = AcMode.Cool,
            TargetTemperatureC = 20,
            FanSpeed = AcFanSpeed.Low,
        };

        var commands = ThinqCommandBuilder.Build(template, FullAcProfile());

        Assert.Equal(4, commands.Count);
        Assert.Equal("POWER_ON", commands[0]["operation"]!["airConOperationMode"]!.GetValue<string>());
        Assert.Equal("COOL", commands[1]["airConJobMode"]!["currentJobMode"]!.GetValue<string>());
        Assert.Equal(20, commands[2]["temperature"]!["targetTemperature"]!.GetValue<int>());
        Assert.Equal("C", commands[2]["temperature"]!["unit"]!.GetValue<string>());
        Assert.Equal("LOW", commands[3]["airFlow"]!["windStrength"]!.GetValue<string>());
    }

    [Fact]
    public void Empty_template_still_powers_on()
    {
        var commands = ThinqCommandBuilder.Build(new Template { Name = "Empty" }, FullAcProfile());

        Assert.Single(commands);
        Assert.Equal("POWER_ON", commands[0]["operation"]!["airConOperationMode"]!.GetValue<string>());
    }

    [Fact]
    public void Skips_power_on_when_profile_lacks_airConOperationMode()
    {
        var noPowerProfile = new DeviceProfile(JsonNode.Parse("""
            { "property": { "airConJobMode": { "currentJobMode": {"mode":["r","w"]} } } }
            """)!);

        var commands = ThinqCommandBuilder.Build(new Template { Name = "x", Mode = AcMode.Cool }, noPowerProfile);

        Assert.Single(commands);
        Assert.True(commands[0].ContainsKey("airConJobMode"));
    }

    [Fact]
    public void Drops_fields_not_in_profile()
    {
        var minimalProfile = new DeviceProfile(JsonNode.Parse("""
            { "property": { "operation": { "airConOperationMode": {"mode":["r","w"]} } } }
            """)!);

        var commands = ThinqCommandBuilder.Build(new Template
        {
            Name = "Everything",
            Mode = AcMode.Cool,
            TargetTemperatureC = 22,
            FanSpeed = AcFanSpeed.High,
        }, minimalProfile);

        Assert.Single(commands);
        Assert.True(commands[0].ContainsKey("operation"));
    }

    [Fact]
    public void Prefers_windStrengthDetail_when_profile_has_both()
    {
        var profile = new DeviceProfile(JsonNode.Parse("""
            { "property": { "airFlow": { "windStrengthDetail": {"mode":["r","w"]}, "windStrength": {"mode":["r","w"]} } } }
            """)!);

        var commands = ThinqCommandBuilder.Build(new Template { Name = "x", FanSpeed = AcFanSpeed.Mid }, profile);

        Assert.Single(commands);
        Assert.True(commands[0]["airFlow"]!.AsObject().ContainsKey("windStrengthDetail"));
        Assert.False(commands[0]["airFlow"]!.AsObject().ContainsKey("windStrength"));
    }

    [Fact]
    public void PowerCommand_emits_operation_resource()
    {
        var off = ThinqCommandBuilder.PowerCommand(AcPower.Off);
        Assert.Equal("POWER_OFF", off["operation"]!["airConOperationMode"]!.GetValue<string>());
    }
}
