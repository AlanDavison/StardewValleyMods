using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DecidedlyShared.Ui;

public class ContainerElement : UiElement
{
    internal List<UiElement> childElements = new List<UiElement>();
    internal int containerMargin;

    public ContainerElement(string name, Rectangle bounds, DrawableType type = DrawableType.SlicedBox, Texture2D? texture = null, Rectangle? sourceRect = null,
        Color? color = null,
        int topEdgeSize = 16, int bottomEdgeSize = 12, int leftEdgeSize = 12, int rightEdgeSize = 16,
        int containerMargin = 4)
        : base(name, bounds, type, texture, sourceRect, color, false,
            topEdgeSize, bottomEdgeSize, leftEdgeSize, rightEdgeSize)
    {
        this.bounds = bounds;
        if (color.HasValue)
            this.textureTint = color.Value;
        else
            this.textureTint = Color.White;

        this.containerMargin = containerMargin;
    }

    internal virtual void AddChild(UiElement child)
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
        base.Draw(spriteBatch);

        foreach (UiElement child in this.childElements)
        {
            child.Draw(spriteBatch);
        }
    }

    public override void ReceiveLeftClick(int x, int y)
    {
        foreach (UiElement child in this.childElements)
        {
            child.ReceiveLeftClick(x, y);
        }

        base.ReceiveLeftClick(x, y);
    }

    public override void ReceiveRightClick(int x, int y)
    {
        foreach (UiElement child in this.childElements)
        {
            child.ReceiveRightClick(x, y);
        }

        base.ReceiveRightClick(x, y);
    }

    internal override void OrganiseChildren()
    {
    }
}
