using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace DecidedlyShared.Ui;

public class UiElement
{
    internal string elementName;
    internal Rectangle bounds;
    internal Rectangle sourceRect = new Rectangle(0, 256, 60, 60);
    internal Texture2D texture = Game1.menuTexture;
    internal Color textureTint;
    internal int topEdgeSize, bottomEdgeSize, leftEdgeSize, rightEdgeSize;
    internal UiElement? parent;

    public UiElement(string name, Rectangle bounds, Texture2D? texture = null, Rectangle? sourceRect = null,
        Color? color = null,
        int topEdgeSize = 4, int bottomEdgeSize = 4, int leftEdgeSize = 4, int rightEdgeSize = 4)
    {
        this.elementName = name;
        this.bounds = bounds;

        if (texture == null)
        {
            this.texture = Game1.menuTexture;
            this.sourceRect = new Rectangle(0, 256, 60, 60);
        }
        else
        {
            this.texture = texture;

            if (sourceRect == null)
                this.sourceRect = this.texture.Bounds;
            else
                this.sourceRect = sourceRect.Value;
        }

        if (color.HasValue)
            this.textureTint = color.Value;
        else
            this.textureTint = Color.White;

        this.topEdgeSize = topEdgeSize;
        this.bottomEdgeSize = bottomEdgeSize;
        this.leftEdgeSize = leftEdgeSize;
        this.rightEdgeSize = rightEdgeSize;
    }

    internal void UpdatePosition(int xPos, int yPos)
    {
        this.BoundsChanged();
    }

    internal void UpdateSize(int width, int height)
    {
        this.BoundsChanged();
    }

    public virtual void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(this.texture, this.bounds, this.sourceRect, this.textureTint);
    }

    internal void BoundsChanged()
    {
    }
}
