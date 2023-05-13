namespace MappingExtensionsAndExtraProperties.Models.TileProperties;

public struct MapBackgroundTileSize : ITilePropertyData
{
    public static string PropertyKey => "MEEP_Background_Tile_Size";
    private int width;
    private int height;

    public int Width => this.width;
    public int Height => this.height;

    public MapBackgroundTileSize(int width, int height)
    {
        this.width = width;
        this.height = height;
    }
}
