using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MappingExtensionsAndExtraProperties.Models.TileProperties;

public struct CloseupInteractionImage
{
    public static string TileProperty = "CloseupInteraction_Image";
    public Texture2D Texture;
    public Rectangle SourceRect;

    public int Width
    {
        get => this.SourceRect.Width;
    }

    public int Height
    {
        get => this.SourceRect.Height;
    }
}
