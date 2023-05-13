namespace MappingExtensionsAndExtraProperties.Models.TileProperties;

public struct LetterText : ITilePropertyData
{
    public static string PropertyKey => "MEEP_Letter";
    public string Text;
}
