namespace MappingExtensionsAndExtraProperties.Models.TileProperties;

public struct MapBackgroundTileVariation : ITilePropertyData
{
    public static string PropertyKey => "MEEP_Background_Tile_Variation";
    private double variationChance;

    public double VariationChance => this.variationChance;

    public MapBackgroundTileVariation(double variation)
    {
        this.variationChance = variation;
    }
}
