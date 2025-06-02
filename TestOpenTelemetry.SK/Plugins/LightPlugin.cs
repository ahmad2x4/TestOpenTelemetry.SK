using System.ComponentModel;
using System.Text.Json.Serialization;
using Microsoft.SemanticKernel;

namespace TestOpenTelemetry.SK.Plugins;

public class LightModel
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("is_on")]
    public bool? IsOn { get; set; }

    [JsonPropertyName("brightness")]
    public Brightness? Brightness { get; set; }

    [JsonPropertyName("color")]
    [Description("The color of the light with a hex code (ensure you include the # symbol)")]
    public string? Color { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Brightness
{
    Low,
    Medium,
    High
}
public class LightsPlugin
{
    private readonly List<LightModel> _lights;
    private readonly ILogger<LightsPlugin> _logger;

    public LightsPlugin(ILogger<LightsPlugin> logger, List<LightModel> lights)
    {
        _logger = logger;
        _lights = lights;
    }

    [KernelFunction("get_lights")]
    [Description("Gets a list of lights and their current state")]
    public async Task<List<LightModel>> GetLightsAsync()
    {
        _logger.LogInformation("Getting all lights. Total count: {LightCount}", _lights.Count);
        await Task.Delay(300);
        return _lights;
    }

    [KernelFunction("change_state")]
    [Description("Changes the state of the light")]
    public async Task<LightModel?> ChangeStateAsync(LightModel changeState)
    {
        _logger.LogInformation("Attempting to change state for light ID: {LightId}", changeState.Id);
        
        // Find the light to change
        var light = _lights.FirstOrDefault(l => l.Id == changeState.Id);
        await Task.Delay(1000);

        // If the light does not exist, return null
        if (light == null)
        {
            _logger.LogWarning("Light with ID {LightId} not found", changeState.Id);
            return null;
        }

        // Update the light state
        light.IsOn = changeState.IsOn;
        light.Brightness = changeState.Brightness;
        light.Color = changeState.Color;

        _logger.LogInformation("Successfully updated light {LightName} (ID: {LightId}). IsOn: {IsOn}, Brightness: {Brightness}, Color: {Color}", 
            light.Name, light.Id, light.IsOn, light.Brightness, light.Color);

        return light;
    }
}
