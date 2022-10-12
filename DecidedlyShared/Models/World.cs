using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley;

namespace DecidedlyShared.Models;

public class World
{
    private GameLocation location;
    private Dictionary<Vector2, WorldTile> tiles;

    public void AddWorldTile(Vector2 tile)
    {
        if (!this.tiles.ContainsKey(tile))
            this.tiles.Add(tile, new WorldTile(tile, this.location));
        else
            this.tiles[tile].UpdateTile();
    }

    public bool TryGetWorldTile(Vector2 tile, out WorldTile? worldTile)
    {
        if (this.tiles.ContainsKey(tile))
        {
            worldTile = this.tiles[tile];
            return true;
        }
        else
        {
            worldTile = null;
            return false;
        }
    }

    public WorldTile GetWorldTile(Vector2 tile)
    {
        return this.tiles[tile];
    }
}
