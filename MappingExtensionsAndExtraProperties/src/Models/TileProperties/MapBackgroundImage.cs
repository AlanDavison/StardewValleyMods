namespace MappingExtensionsAndExtraProperties.Models.TileProperties;

public struct MapBackgroundImage : ITilePropertyData
{
    public static string PropertyKey => "MEEP_CloseupInteraction_Image";
    private string imagePath;

    public string ImagePath => this.imagePath;

    public MapBackgroundImage(string imagePath)
    {
        this.imagePath = imagePath;
    }
}
