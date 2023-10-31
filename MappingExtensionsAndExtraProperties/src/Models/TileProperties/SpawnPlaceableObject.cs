using System;
using Microsoft.Win32.SafeHandles;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Objects;

namespace MappingExtensionsAndExtraProperties.Models.TileProperties;

public struct SpawnPlaceableObject : ITilePropertyData
{
    public static string PropertyKey => "MEEP_SpawnPlaceableObject";
    public bool Breakable = false;
    public Item? bigCraftable;
    private int objectId;

    public SpawnPlaceableObject(int objectId, Item bigCraftable, bool breakable = true)
    {
        this.objectId = objectId;
        this.Breakable = breakable;
        this.bigCraftable = bigCraftable;
    }
}
