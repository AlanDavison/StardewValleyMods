using System.Collections.Generic;

namespace ZoomPerMap;

public class ModConfig
{
    public float defaultOutdoorZoomLevel = 1f;
    public float defaultIndoorZoomLevel = 1.5f;
    public Dictionary<string, float> zoomLevels = new Dictionary<string, float>();
}
