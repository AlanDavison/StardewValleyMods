using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MappingExtensionsAndExtraProperties.Models.TileProperties;

public struct CloseupInteractionImage : ITilePropertyData
{
    public static string PropertyKey => "MEEP_CloseupInteraction_Image";
    public static string ReelProperty => "MEEP_CloseupInteraction_Image_1";
    public Texture2D Texture;
    public Rectangle SourceRect;
    private ITilePropertyData tilePropertyDataImplementation;

    public int Width
    {
        get => this.SourceRect.Width;
    }

    public int Height
    {
        get => this.SourceRect.Height;
    }

}
