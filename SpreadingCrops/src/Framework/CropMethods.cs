using System.Collections.Generic;
using DecidedlyShared.Utilities;
using Microsoft.Xna.Framework;
using SpreadingCrops.Framework.Models;
using StardewValley;
using StardewValley.GameData.Crops;
using StardewValley.TerrainFeatures;

namespace SpreadingCrops.Framework;

public class CropMethods
{
    public static void TrySpreadCrop(HoeDirt cropDirt, SpreadingCropInfo info)
    {
        Vector2 tile = cropDirt.Tile;
        List<Vector2> surroundingTiles = Locations.GetSurroundingTileCoords(tile);
        List<Vector2> validTiles = ValidTiles(surroundingTiles, cropDirt.Location);

        if (validTiles.Count == 0)
            return;

        if (!info.SpreadToAllValidTiles)
        {
            int selectedTile = Game1.random.Next(0, validTiles.Count - 1);

            SpreadCropToTile(cropDirt, cropDirt.Location, validTiles[selectedTile]);
        }
        else
        {
            foreach (Vector2 currentTile in validTiles)
            {
                SpreadCropToTile(cropDirt, cropDirt.Location, currentTile);
            }
        }
    }

    private static void SpreadCropToTile(HoeDirt originalCropDirt, GameLocation location, Vector2 tile)
    {
        location.makeHoeDirt(tile);

        if (!location.terrainFeatures.ContainsKey(tile))
            return;

        HoeDirt dirt = location.GetHoeDirtAtTile(tile);
        dirt.plant(originalCropDirt.crop.netSeedIndex.Value, Game1.player, false);
    }

    public static List<Vector2> ValidTiles(List<Vector2> tiles, GameLocation location)
    {
        List<Vector2> validatedTiles = tiles;
        List<Vector2> invalidTiles = new List<Vector2>();

        foreach (Vector2 tile in tiles)
        {
            if (location.Objects.ContainsKey(tile))
            {
                invalidTiles.Add(tile);
                continue;
            }

            if (location.terrainFeatures.ContainsKey(tile))
            {
                invalidTiles.Add(tile);
                continue;
            }

            if (!location.isTileLocationOpen(tile))
            {
                invalidTiles.Add(tile);
                continue;
            }
        }

        foreach (Vector2 invalid in invalidTiles)
        {
            validatedTiles.Remove(invalid);
        }

        return validatedTiles;
    }
}
