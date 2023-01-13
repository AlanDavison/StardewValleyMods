using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DecidedlyShared.Ui;

public class DraggableElement : UiElement
{
    private bool currentlyBeingDragged;
    private UiElement dragArea;

    public DraggableElement(string name, Rectangle bounds, DrawableType type = DrawableType.Texture,Texture2D? texture = null, Rectangle? sourceRect = null,
        Color? color = null)
        : base(name, bounds, type, texture, sourceRect, color)
    {
    }
}
