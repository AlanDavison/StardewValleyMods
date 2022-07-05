using System.Collections.Generic;
using Microsoft.Xna.Framework;
using SmartBuilding.APIs;
using SmartBuilding.Logging;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using SObject = StardewValley.Object;

namespace SmartBuilding.Utilities
{
    public class PlacementUtils
    {
        private ModConfig config;
        private IdentificationUtils identificationUtils;
        private IMoreFertilizersAPI? moreFertilizersApi;
        private Logger logger;
        private IModHelper helper;
        
        public PlacementUtils(ModConfig config, IdentificationUtils identificationUtils, IMoreFertilizersAPI moreFertilizersApi, Logger logger, IModHelper helper)
        {
            this.config = config;
            this.identificationUtils = identificationUtils;
            this.moreFertilizersApi = moreFertilizersApi;
            this.logger = logger;
            this.helper = helper;
        }

        public bool HasAdjacentNonWaterTile(Vector2 v)
        {
            // Right now, this is only applicable for crab pots.
            if (config.CrabPotsInAnyWaterTile)
                return true;

            // We create our list of cardinal and ordinal directions.
            List<Vector2> directions = new List<Vector2>()
            {
                v + new Vector2(-1, 0), // Left
                v + new Vector2(1, 0), // Right
                v + new Vector2(0, -1), // Up
                v + new Vector2(0, 1), // Down
                v + new Vector2(-1, -1), // Up left
                v + new Vector2(1, -1), // Up right
                v + new Vector2(-1, 1), // Down left
                v + new Vector2(1, 1) // Down right
            };

            // Then loop through in each of those directions relative to the passed in tile to determine if a water tile is adjacent.
            foreach (Vector2 vector in directions)
            {
                if (!Game1.currentLocation.isWaterTile((int)vector.X, (int)vector.Y))
                    return true;
            }

            return false;
        }
        
        /// <summary>
        /// Will return whether or not a tile can be placed 
        /// </summary>
        /// <param name="v">The world-space Tile in which the check is to be performed.</param>
        /// <param name="i">The placeable type.</param>
        /// <returns></returns>
        public bool CanBePlacedHere(Vector2 v, Item i)
        {
            // // If the item is a tool, we want to return.
            // if (i is Tool)
            //     return false;

            ItemType itemType = identificationUtils.IdentifyItemType((Object)i);
            ItemInfo itemInfo = identificationUtils.GetItemInfo((Object)i);
            GameLocation here = Game1.currentLocation;

            switch (itemType)
            {
                case ItemType.NotPlaceable:
                    return false;
                case ItemType.Torch:
                    // We need to figure out whether there's a fence in the placement tile.
                    if (here.objects.ContainsKey(v))
                    {
                        // We know there's an object at these coordinates, so we grab a reference.
                        SObject o = here.objects[v];

                        // Then we return true if it's a fence, because we want to place the torch on the fence.
                        if (identificationUtils.IsTypeOfObject(o, ItemType.Fence))
                        {
                            // It's a type of fence, but we also want to ensure that it isn't a gate.

                            if (o.Name.Equals("Gate"))
                                return false;

                            return true;
                        }
                    }
                    else
                        goto GenericPlaceable; // Please don't hate me too much. This is temporary until everything gets split out into separate methods eventually.

                    break;
                case ItemType.CrabPot: // We need to determine if the crab pot is being placed in an appropriate water tile.
                    return CrabPot.IsValidCrabPotLocationTile(here, (int)v.X, (int)v.Y) && HasAdjacentNonWaterTile(v);
                case ItemType.GrassStarter:
                    // If there's a terrain feature here, we can't possibly place a grass starter.
                    return !here.terrainFeatures.ContainsKey(v);
                case ItemType.Floor:
                    // In this case, we need to know whether there's a TerrainFeature in the tile.
                    if (here.terrainFeatures.ContainsKey(v))
                    {
                        // At this point, we know there's a terrain feature here, so we grab a reference to it.
                        TerrainFeature tf = Game1.currentLocation.terrainFeatures[v];

                        // Then we check to see if it is, indeed, Flooring.
                        if (tf != null && tf is Flooring)
                        {
                            // If it is, and if the setting to replace floors with floors is enabled, we return true.
                            if (config.EnableReplacingFloors)
                                return true;
                        }

                        return false;
                    }
                    else if (here.objects.ContainsKey(v))
                    {
                        // We know an object exists here now, so we grab it.
                        SObject o = here.objects[v];
                        ItemType type;
                        Item itemToDestroy;

                        itemToDestroy = Utility.fuzzyItemSearch(o.Name);
                        type = identificationUtils.IdentifyItemType((SObject)itemToDestroy);

                        if (type == ItemType.Fence)
                        {
                            // This is a fence, so we return true.

                            return true;
                        }
                    }

                    // At this point, we return appropriately with vanilla logic, or true depending on the placement setting.
                    return config.LessRestrictiveFloorPlacement || here.isTileLocationTotallyClearAndPlaceable(v);
                case ItemType.Chest:
                    goto case ItemType.Generic;
                case ItemType.Fertilizer:
                    // If the setting to enable fertilizers is off, return false to ensure they can't be added to the queue.
                    if (!config.EnableCropFertilizers)
                        return false;

                    // If this is a More Fertilizers fertilizer, defer to More Fertilizer's placement logic.
                    if (i is SObject obj && moreFertilizersApi?.CanPlaceFertilizer(obj, here, v) == true)
                        return true;

                    // If there's an object present, we don't want to place any fertilizer.
                    // It is technically valid, but there's no reason someone would want to.
                    if (here.Objects.ContainsKey(v))
                        return false;

                    if (here.terrainFeatures.ContainsKey(v))
                    {
                        // We know there's a TerrainFeature here, so next we want to check if it's HoeDirt.
                        if (here.terrainFeatures[v] is HoeDirt)
                        {
                            // If it is, we want to grab the HoeDirt, and check for the possibility of planting.
                            HoeDirt hd = (HoeDirt)here.terrainFeatures[v];

                            if (hd.crop != null)
                            {
                                // If the HoeDirt has a crop, we want to grab it and check for growth phase and fertilization status.
                                Crop cropToCheck = hd.crop;

                                if (cropToCheck.currentPhase.Value != 0)
                                {
                                    // If the crop's current phase is not zero, we return false.

                                    return false;
                                }
                            }

                            // At this point, we fall to vanilla logic to determine placement validity.
                            return hd.canPlantThisSeedHere(i.ParentSheetIndex, (int)v.X, (int)v.Y, true);
                        }
                    }

                    return false;
                case ItemType.TreeFertilizer:
                    // If the setting to enable tree fertilizers is off, return false to ensure they can't be added to the queue.
                    if (!config.EnableTreeFertilizers)
                        return false;

                    // First, we determine if there's a TerrainFeature here.
                    if (here.terrainFeatures.ContainsKey(v))
                    {
                        // Then we check if it's a tree.
                        if (here.terrainFeatures[v] is Tree)
                        {
                            // It is a tree, so now we check to see if the tree is fertilised.
                            Tree tree = (Tree)here.terrainFeatures[v];

                            // If it's already fertilised, there's no need for us to want to place tree fertiliser on it, so we return false.
                            if (tree.fertilized.Value)
                                return false;
                            else
                                return true;
                        }
                    }

                    return false;
                case ItemType.Seed:
                    // If the setting to enable crops is off, return false to ensure they can't be added to the queue.
                    if (!config.EnablePlantingCrops)
                        return false;

                    // If there's an object present, we don't want to place a seed.
                    // It is technically valid, but there's no reason someone would want to.
                    if (here.Objects.ContainsKey(v))
                        return false;

                    // First, we check for a TerrainFeature.
                    if (here.terrainFeatures.ContainsKey(v))
                    {
                        // Then, we check to see if it's HoeDirt.
                        if (here.terrainFeatures[v] is HoeDirt)
                        {
                            // Next, we check to see if it's a DGA item.
                            if (itemInfo.IsDgaItem)
                            {
                                // It is, so we try to use DGA to determine plantability. 

                                // First, we grab a reference to our HoeDirt.
                                HoeDirt hd = (HoeDirt)here.terrainFeatures[v];

                                // Then reflect into DGA to get the CanPlantThisSeedHere method.
                                IReflectedMethod canPlant = helper.Reflection.GetMethod(
                                    i,
                                    "CanPlantThisSeedHere"
                                );

                                return canPlant.Invoke<bool>(new[] { (object)hd, (int)v.X, (int)v.Y, false });

                                // And we return false here if the reflection failed, because we couldn't determine plantability.
                                logger.Log("Reflecting into DGA to determine seed plantability failed. Please DO NOT report this to spacechase0.", LogLevel.Error);
                                return false;
                            }
                            else
                            {
                                // If it is, we grab a reference to the HoeDirt to use its canPlantThisSeedHere method.
                                HoeDirt hd = (HoeDirt)here.terrainFeatures[v];

                                return hd.canPlantThisSeedHere(i.ParentSheetIndex, (int)v.X, (int)v.Y);
                            }
                        }
                    }

                    return false;
                case ItemType.Tapper:
                    // If the setting to enable tree tappers is off, we return false here to ensure nothing further happens.
                    if (!config.EnableTreeTappers)
                        return false;

                    // First, we need to check if there's a TerrainFeature here.
                    if (here.terrainFeatures.ContainsKey(v))
                    {
                        // If there is, we check to see if it's a tree.
                        if (here.terrainFeatures[v] is Tree)
                        {
                            // If it is, we grab a reference to the tree to check its details.
                            Tree tree = (Tree)here.terrainFeatures[v];

                            // If the tree isn't tapped, we confirm that a tapper can be placed here.
                            if (!tree.tapped)
                            {
                                // If the tree is fully grown, we *can* place a tapper.
                                return tree.growthStage >= 5;
                            }
                        }
                    }

                    return false;
                case ItemType.Fence:
                    // We want to deal with fences specifically in order to handle fence replacements.
                    if (here.objects.ContainsKey(v))
                    {
                        // We know there's an object at these coordinates, so we grab a reference.
                        SObject o = here.objects[v];

                        // Then we return true if this is both a fence, and replacing fences is enabled.
                        return identificationUtils.IsTypeOfObject(o, ItemType.Fence) && config.EnableReplacingFences;
                    }
                    else if (here.terrainFeatures.ContainsKey(v))
                    {
                        // There's a terrain feature here, so we want to check if it's a HoeDirt with a crop.
                        TerrainFeature feature = here.terrainFeatures[v];

                        if (feature != null && feature is HoeDirt)
                        {
                            if ((feature as HoeDirt).crop != null)
                            {
                                // There's a crop here, so we return false.
                                return false;
                            }

                            // At this point, we know it's a HoeDirt, but has no crop, so we can return true.
                            return true;
                        }
                    }

                    goto case ItemType.Generic;
                case ItemType.FishTankFurniture:
                    // Fishtank furniture is locked out until I figure out how to transplant fish correctly.
                    return false;
                case ItemType.StorageFurniture:
                    // Since FishTankFurniture will sneak through here:
                    if (i is FishTankFurniture)
                        return false;

                    // If the setting for allowing storage furniture is off, we get the hell out.
                    if (!config.EnablePlacingStorageFurniture && !itemInfo.IsDgaItem)
                        return false;

                    if (config.LessRestrictiveFurniturePlacement)
                        return true;
                    else
                    {
                        return (i as StorageFurniture).canBePlacedHere(here, v);
                    }
                case ItemType.TvFurniture:
                    if (config.LessRestrictiveFurniturePlacement && !itemInfo.IsDgaItem)
                        return true;
                    else
                    {
                        return (i as TV).canBePlacedHere(here, v);
                    }
                case ItemType.BedFurniture:
                    if (config.LessRestrictiveBedPlacement && !itemInfo.IsDgaItem)
                        return true;
                    else
                    {
                        return (i as BedFurniture).canBePlacedHere(here, v);
                    }
                case ItemType.GenericFurniture:
                    // In this place, we play fast and loose, and return true.
                    if (config.LessRestrictiveFurniturePlacement && !itemInfo.IsDgaItem)
                        return true;
                    else
                    {
                        return (i as Furniture).canBePlacedHere(here, v);
                    }
                case ItemType.Generic:
                    GenericPlaceable: // A goto, I know, gross, but... it works, and is fine for now, until I split out detection logic into methods.

                    if (config.LessRestrictiveObjectPlacement)
                    {
                        // If the less restrictive object placement setting is enabled, we first want to check if vanilla logic dictates the object be placeable.
                        if (Game1.currentLocation.isTileLocationTotallyClearAndPlaceableIgnoreFloors(v))
                        {
                            // It dictates that it is, so we can simply return true.
                            return true;
                        }
                        else
                        {
                            // Otherwise, we want to check for an object already present in this location.
                            if (!here.Objects.ContainsKey(v))
                            {
                                // There is no object here, so we return true, as we should be able to place the object here.
                                return true;
                            }

                            // We could just fall through to vanilla logic again at this point, but that would be vaguely pointless, so we just return false.
                            return false;
                        }
                    }
                    return Game1.currentLocation.isTileLocationTotallyClearAndPlaceableIgnoreFloors(v);
            }

            // If the PlaceableType is somehow none of these, we want to be safe and return false.
            return false;
        }
    }
}