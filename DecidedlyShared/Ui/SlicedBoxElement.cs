using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DecidedlyShared.Ui;

public class SlicedBoxElement : UiElement
{
    public SlicedBoxElement(string name, Rectangle bounds, Texture2D? texture = null, Rectangle? sourceRect = null, Color? color = null, int topEdgeSize = 4, int bottomEdgeSize = 4, int leftEdgeSize = 4, int rightEdgeSize = 4) : base(name, bounds, texture, sourceRect, color, topEdgeSize, bottomEdgeSize, leftEdgeSize, rightEdgeSize)
    {
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        Utils.DrawBox(
            spriteBatch,
            this.texture,
            this.sourceRect,
            this.bounds,
            this.topEdgeSize,
            this.leftEdgeSize,
            this.rightEdgeSize,
            this.bottomEdgeSize
        );
    }
}
