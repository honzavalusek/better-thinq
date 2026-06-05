using System.Net;
using System.Text.Json.Nodes;
using BetterThinq.ThinQ.Models;
using Microsoft.Extensions.Logging.Abstractions;

namespace BetterThinq.ThinQ.Tests;

public class ThinqClientTests
{
    [Fact]
    public async Task GetDevices_parses_response_wrapper()
    {
        var json = await File.ReadAllTextAsync("Fixtures/devices_list.json");
        var client = MakeClient(FakeHttpHandler.Json(json));

        var devices = await client.GetDevicesAsync();

        Assert.Equal(2, devices.Count);
        Assert.Equal("ac-living-room-1", devices[0].DeviceId);
        Assert.Equal("Living Room AC", devices[0].Alias);
        Assert.Equal("DEVICE_AIR_CONDITIONER", devices[0].DeviceType);
    }

    [Fact]
    public async Task GetDevices_parses_data_wrapper()
    {
        const string json = """
            { "code": 2000, "data": [
                { "deviceId": "x", "deviceInfo": { "alias": "X", "modelName": "M", "deviceType": "DEVICE_AIR_CONDITIONER" } }
            ] }
            """;
        var client = MakeClient(FakeHttpHandler.Json(json));

        var devices = await client.GetDevicesAsync();

        Assert.Single(devices);
        Assert.Equal("x", devices[0].DeviceId);
    }

    [Fact]
    public async Task GetDevices_returns_empty_when_shape_is_unexpected()
    {
        var client = MakeClient(FakeHttpHandler.Json("""{"messageId":"x","response":{"oops":true}}"""));
        var devices = await client.GetDevicesAsync();
        Assert.Empty(devices);
    }

    [Fact]
    public async Task GetDeviceAsync_returns_state_with_helpers()
    {
        var json = await File.ReadAllTextAsync("Fixtures/ac_state.json");
        var client = MakeClient(FakeHttpHandler.Json(json));

        var state = await client.GetDeviceAsync("ac-1");

        Assert.Equal("POWER_ON", state.StringAt("operation.airConOperationMode"));
        Assert.Equal(22, state.NumberAt("temperature.targetTemperature"));
        Assert.Null(state.StringAt("nonexistent.field"));
    }

    [Fact]
    public async Task GetProfileAsync_returns_profile_supporting_HasKey()
    {
        var json = await File.ReadAllTextAsync("Fixtures/ac_profile.json");
        var client = MakeClient(FakeHttpHandler.Json(json));

        var profile = await client.GetProfileAsync("ac-1");

        Assert.True(profile.HasKey("airConJobMode"));
        Assert.True(profile.HasKey("targetTemperature"));
        Assert.True(profile.HasKey("windStrength"));
        Assert.False(profile.HasKey("madeUpField"));
    }

    [Fact]
    public async Task SendCommandAsync_posts_to_control_endpoint()
    {
        var fake = FakeHttpHandler.Json("{}");
        var client = MakeClient(fake);

        var cmd = new JsonObject { ["operation"] = new JsonObject { ["airConOperationMode"] = "POWER_ON" } };
        await client.SendCommandAsync("dev-id-42", cmd);

        var req = fake.Requests.Single();
        Assert.Equal(HttpMethod.Post, req.Method);
        Assert.EndsWith("/devices/dev-id-42/control", req.RequestUri!.AbsolutePath);
        var body = await req.Content!.ReadAsStringAsync();
        Assert.Contains("POWER_ON", body);
    }

    [Fact]
    public async Task SendCommandAsync_throws_on_non_success()
    {
        var fake = FakeHttpHandler.Json("""{"error":"bad mode"}""", HttpStatusCode.BadRequest);
        var client = MakeClient(fake);

        var ex = await Assert.ThrowsAsync<ThinqException>(() =>
            client.SendCommandAsync("d", new JsonObject()));

        Assert.Equal(HttpStatusCode.BadRequest, ex.Status);
        Assert.Contains("bad mode", ex.Body);
    }

    [Fact]
    public async Task GetDevices_throws_on_500()
    {
        var responses = new[]
        {
            new HttpResponseMessage(HttpStatusCode.InternalServerError) { Content = new StringContent("boom") },
            new HttpResponseMessage(HttpStatusCode.InternalServerError) { Content = new StringContent("boom") },
            new HttpResponseMessage(HttpStatusCode.InternalServerError) { Content = new StringContent("boom") },
            new HttpResponseMessage(HttpStatusCode.InternalServerError) { Content = new StringContent("boom") },
        };
        var client = MakeClient(FakeHttpHandler.Sequence(responses), withRetry: false);

        var ex = await Assert.ThrowsAsync<ThinqException>(() => client.GetDevicesAsync());
        Assert.Equal(HttpStatusCode.InternalServerError, ex.Status);
    }

    private static IThinqClient MakeClient(FakeHttpHandler fake, bool withRetry = false)
    {
        HttpMessageHandler handler = fake;
        if (withRetry) handler = new ThinqRetryHandler { InnerHandler = fake };
        var http = new HttpClient(handler) { BaseAddress = new Uri("https://api-eic.lgthinq.com/") };
        return new ThinqClient(http, NullLogger<ThinqClient>.Instance);
    }
}
