namespace MappingExtensionsAndExtraProperties.Models.TileProperties;

public struct LetterType : ITilePropertyData
{
    public static string PropertyKey => "MEEP_Letter_Type";
    public int Type;
}
