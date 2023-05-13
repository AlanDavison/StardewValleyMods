namespace MappingExtensionsAndExtraProperties.Models.TileProperties;

public struct DhFakeNpc : ITilePropertyData
{
    private string npcName;

    public static string PropertyKey => "MEEP_FakeNPC";
    public int SpriteWidth;
    public int SpriteHeight;
    public bool HasSpriteSizes;
    public string NpcName;
}
