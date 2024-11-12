using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using StardewValley;

namespace MappingExtensionsAndExtraProperties.Models.WarpStations;

public class WarpStationTarget
{
    /// <summary>
    /// The ID for the warp station target.
    /// </summary>
    private string targetId;

    /// <summary>
    /// The GameLocation instance to be warped to.
    /// </summary>
    private GameLocation targetLocation;

    /// <summary>
    /// The internal ID used to get <see cref="targetLocation"/>'s <see cref="GameLocation"/> instance.
    /// </summary>
    [JsonProperty("Location")]
    private string targetLocationId;

    /// <summary>
    /// The <see cref="Vector2"/> tile to warp the player to.
    /// </summary>
    [JsonProperty("Tile")]
    private Vector2 targetTile;

    /// <summary>
    /// The <see cref="Texture2D"/> image displayed in the warp target's UI.
    /// </summary>
    private Texture2D displayImage;

    /// <summary>
    /// The asset name to pull the location's <see cref="displayImage"/> from.
    /// </summary>
    [JsonProperty("Image")]
    private string displayImageAsset;

    /// <summary>
    /// The description of the location to be displayed in the UI.
    /// </summary>
    [JsonProperty("Description")]
    private string targetDescription;

    /// <summary>
    /// The event to be played when departing to this target location, if any. Overrides the
    /// warp station's departing event.
    /// </summary>
    [JsonProperty("DepartingEvent")]
    private string? departingEvent;

    /// <summary>
    /// The event to be played when arriving at this target location, if any.
    /// </summary>
    [JsonProperty("ArrivingEvent")]
    private string? arrivingEvent;
}
