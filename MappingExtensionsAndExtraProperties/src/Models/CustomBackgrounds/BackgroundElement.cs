using Newtonsoft.Json;

namespace MappingExtensionsAndExtraProperties.Models.CustomBackgrounds;

public class BackgroundElement
{
    public string? ImageId { get; set; }
    [JsonProperty("BaseX")]
    public int BaseX { get; set; }
    [JsonProperty("BaseY")]
    public int BaseY { get; set; }
    [JsonIgnore] public float x;
    [JsonIgnore] public float y;
    public int HorizontalParallaxFactor { get; set; }
    public int VerticalParallaxFactor { get; set; }
    public int? Depth { get; set; }
    public string? Anchor { get; set; }
}
