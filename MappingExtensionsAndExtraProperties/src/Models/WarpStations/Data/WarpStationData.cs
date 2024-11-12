using System.Collections.Generic;
using Newtonsoft.Json;
using StardewValley;

namespace MappingExtensionsAndExtraProperties.Models.WarpStations;

public class WarpStationData
{
    /// <summary>
    /// The title for the warp station UI.
    /// </summary>
    [JsonProperty("Title")]
    private string stationTitle;

    /// <summary>
    /// The <see cref="WarpStationTarget"/> targets being handled by this warp station.
    /// </summary>
    [JsonProperty("Targets")]
    private Dictionary<string, WarpStationTarget> warpTargets;

    /// <summary>
    /// The event to be played when departing this location. Can be overridden by <see cref="WarpStationTarget"/>'s departingEvent field.
    /// </summary>
    [JsonProperty("DepartingEvent")]
    private string departingEvent;
}
