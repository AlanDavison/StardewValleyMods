using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DecidedlyShared.Ui;

public class ContainerElement : UiElement
{
    internal List<UiElement> childElements = new List<UiElement>();
    internal int containerMargin;
    internal bool drawBox;

    public ContainerElement(string name, Rectangle bounds, bool drawBox = false, Texture2D? texture = null, Rectangle? sourceRect = null,
        Color? color = null,
        int topEdgeSize = 4, int bottomEdgeSize = 4, int leftEdgeSize = 4, int rightEdgeSize = 4,
        int containerMargin = 4)
        : base(name, bounds, texture, sourceRect, color,
            topEdgeSize, bottomEdgeSize, leftEdgeSize, rightEdgeSize)
    {
        this.bounds = bounds;
        if (color.HasValue)
            this.textureTint = color.Value;
        else
            this.textureTint = Color.White;

        this.containerMargin = containerMargin;
        this.drawBox = drawBox;
    }

    internal void AddChild(UiElement child)
    {
        if (!this.childElements.Contains(child))
        {
            this.childElements.Add(child);
            child.parent = this;
        }

        this.OrganiseChildren();
    }

    internal virtual void Draw(SpriteBatch spriteBatch)
    {
        if (this.drawBox)
        {
            Utils.DrawBox(
                spriteBatch,
                this.texture,
                this.sourceRect,
                this.bounds,
                this.topEdgeSize,
                this.leftEdgeSize,
                this.rightEdgeSize,
                this.bottomEdgeSize);
        }

        // base.Draw(spriteBatch);

        foreach (UiElement child in this.childElements)
        {
            child.Draw(spriteBatch);
        }
    }

    internal virtual void OrganiseChildren()
    {
    }
}
