using System;
using DecidedlyShared.Logging;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using StardewValley;
using xTile;
using xTile.Dimensions;
using xTile.Layers;
using xTile.ObjectModel;
using xTile.Tiles;

namespace DecidedlyShared.Utilities;

public class TilePropertyHandler
{
    private Logger logger;

    public TilePropertyHandler(Logger logger)
    {
        this.logger = logger;
    }

    public bool TryGetBackProperty(int x, int y, GameLocation location, string key,
        out PropertyValue tileProperty)
    {
        return this.TryGetTileProperty(x, y, location, "Back", key, out tileProperty);
    }

    public bool TryGetBuildingProperty(int x, int y, GameLocation location, string key,
        out PropertyValue tileProperty)
    {
        return this.TryGetTileProperty(x, y, location, "Buildings", key, out tileProperty);
    }

    public bool TryGetTileProperty(int x, int y, GameLocation location, string layer, string key,
        out PropertyValue tileProperty)
    {
        // We need a default assignment.
        tileProperty = null;

        // // Grab our map reference to check for null.
        // Map map = location.Map;
        // if (map == null)
        // {
        //     this.logger.Error($"{location.Name}'s map was somehow null. This is spooky.");
        //
        //     return false;
        // }
        //
        // // Then our layer.
        // Layer desiredLayer = map.GetLayer(layer);
        // if (desiredLayer == null)
        // {
        //     this.logger.Error($"${location.Name}'s {layer} layer didn't exist. This is spooky.");
        //
        //     return false;
        // }
        //
        // // And, finally, our tile.
        // Tile desiredTile = desiredLayer.PickTile(new Location(x * 64, y * 64), Game1.viewport.Size);
        // if (desiredTile == null)
        // {
        //     this.logger.Error($"Tile {x}, {y} in {location.Name} on layer {layer} ");
        //
        //     return false;
        // }
        //
        // // Check if the tile has any properties, or if its Properties property is null.
        // if (desiredTile?.Properties == null || desiredTile.Properties.Count == 0)
        //     return false;
        //
        // // It's not, so we can safely move forward.
        // desiredTile.Properties.TryGetValue(key, out tileProperty);

        return location.Map?.GetLayer(layer)?.PickTile(new Location(x * 64, y * 64), Game1.viewport.Size)?.Properties
            ?.TryGetValue(key, out tileProperty) != false;

        // return location.Map.GetLayer(layer).PickTile(new Location(x * 64, y * 64), Game1.viewport.Size).Properties
        //     .TryGetValue(key, out tileProperty);
    }
}
