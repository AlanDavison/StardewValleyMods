using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DecidedlyShared.Ui;

public class ImageElement : UiElement
{
    public ImageElement(string name, Rectangle bounds, Texture2D? texture = null, Rectangle? sourceRect = null,
        Color? color = null, int topEdgeSize = 4, int bottomEdgeSize = 4, int leftEdgeSize = 4, int rightEdgeSize = 4) :
        base(name, bounds, texture, sourceRect, color, topEdgeSize, bottomEdgeSize, leftEdgeSize, rightEdgeSize)
    {
    }
}
