using System.Text.Json.Nodes;
using BetterThinq.ThinQ.Models;
using BetterThinq.Web.Domain;

namespace BetterThinq.Web.ThinQ;

public static class ThinqCommandBuilder
{
    // LG's ThinQ Connect API accepts only one resource per control call.
    // We emit a separate payload per template field and the caller sends them in turn.
    // Applying a template always powers the AC on first, regardless of template fields.
    public static List<JsonObject> Build(Template template, DeviceProfile profile)
    {
        var commands = new List<JsonObject>();

        if (profile.HasKey("airConOperationMode"))
        {
            commands.Add(PowerCommand(AcPower.On));
        }

        if (template.Mode is { } mode && profile.HasKey("airConJobMode"))
        {
            commands.Add(new JsonObject
            {
                ["airConJobMode"] = new JsonObject
                {
                    ["currentJobMode"] = mode.ToThinq(),
                },
            });
        }

        if (template.TargetTemperatureC is { } temp && profile.HasKey("targetTemperature"))
        {
            commands.Add(new JsonObject
            {
                ["temperature"] = new JsonObject
                {
                    ["targetTemperature"] = temp,
                    ["unit"] = "C",
                },
            });
        }

        if (template.FanSpeed is { } fan)
        {
            // Profiles may expose either windStrengthDetail (newer) or windStrength.
            var windKey = profile.HasKey("windStrengthDetail") ? "windStrengthDetail"
                        : profile.HasKey("windStrength") ? "windStrength"
                        : null;
            if (windKey is not null)
            {
                commands.Add(new JsonObject
                {
                    ["airFlow"] = new JsonObject
                    {
                        [windKey] = fan.ToThinq(),
                    },
                });
            }
        }

        return commands;
    }

    public static JsonObject PowerCommand(AcPower power) => new()
    {
        ["operation"] = new JsonObject
        {
            ["airConOperationMode"] = power.ToThinq(),
        },
    };

}
