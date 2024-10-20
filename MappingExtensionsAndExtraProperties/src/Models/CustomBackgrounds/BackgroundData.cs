using System.Collections.Generic;

namespace MappingExtensionsAndExtraProperties.Models.CustomBackgrounds;

public class BackgroundData
{
    public string? Format { get; set; }
    public List<BackgroundImage>? Images { get; set; }
    public BackgroundScene? Scene { get; set; }
}
