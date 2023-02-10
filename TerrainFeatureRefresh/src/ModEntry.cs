using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace TerrainFeatureRefresh;

public class ModEntry : Mod
{
    public override void Entry(IModHelper helper)
    {
        helper.Events.Input.ButtonPressed += (sender, args) =>
        {
            if (args.IsDown(SButton.OemSemicolon))
            {
                // No good. Gets the same, active instance, not a new one.
                // GameLocation location = Game1.getLocationFromName(Game1.currentLocation.Name);
                // GameLocation curLoc = Game1.currentLocation;

                // TODO: Change this to use GameLocation.CreateGameLocation(string id) instead.
                GameLocation location = new GameLocation(Game1.currentLocation.mapPath.Value, Game1.currentLocation.Name);
                // GameLocation curLoc = Game1.currentLocation;

                List<Vector2> objectsToRemove = new();
                foreach(Vector2 tile in Game1.currentLocation.Objects.Keys)
                {
                    objectsToRemove.Add(tile);
                }

                foreach (Vector2 tile in objectsToRemove)
                {
                    Game1.currentLocation.Objects.Remove(tile);
                }

                List<Vector2> terrainFeaturesToRemove = new();
                foreach (Vector2 tile in Game1.currentLocation.terrainFeatures.Keys)
                {
                    terrainFeaturesToRemove.Add(tile);
                }

                List<LargeTerrainFeature> largeTerrainFeaturesToRemove = new();
                foreach (LargeTerrainFeature feature in Game1.currentLocation.largeTerrainFeatures)
                {
                    largeTerrainFeaturesToRemove.Add(feature);
                }

                foreach (LargeTerrainFeature tile in largeTerrainFeaturesToRemove)
                {
                    Game1.currentLocation.largeTerrainFeatures.Remove(tile);
                }

                foreach (Vector2 tile in terrainFeaturesToRemove)
                {
                    Game1.currentLocation.terrainFeatures.Remove(tile);
                }

                foreach (Vector2 tile in location.Objects.Keys)
                {
                    if (location.Objects.TryGetValue(tile, out SObject obj))
                    {
                        Game1.currentLocation.Objects.Add(tile, obj);
                    }
                }

                foreach (Vector2 tile in location.terrainFeatures.Keys)
                {
                    if (location.terrainFeatures.TryGetValue(tile, out TerrainFeature tf))
                        Game1.currentLocation.terrainFeatures.Add(tile, tf);
                }

                foreach (LargeTerrainFeature feature in location.largeTerrainFeatures)
                {
                    Game1.currentLocation.largeTerrainFeatures.Add(feature);
                }
            }
        };
    }
}
