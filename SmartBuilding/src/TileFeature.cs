namespace SmartBuilding
{
    /// <summary>
    /// The type of feature we're working with.
    /// This is required so we know whether to target <see cref="StardewValley.GameLocation.Objects"/>,
    /// or <see cref="StardewValley.GameLocation.terrainFeatures"/>.
    /// </summary>
    public enum TileFeature
    {
        /// <summary>
        /// A <see cref="StardewValley.Object"/>.
        /// </summary>
        Object,

        /// <summary>
        /// A <see cref="StardewValley.TerrainFeatures.TerrainFeature"/>.
        /// </summary>
        TerrainFeature,

        /// <summary>
        /// A <see cref="StardewValley.Objects.Furniture"/>
        /// </summary>
        Furniture,
    }
}