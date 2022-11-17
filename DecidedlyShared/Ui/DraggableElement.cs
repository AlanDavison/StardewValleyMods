using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DecidedlyShared.Ui;

public class DraggableElement : ClickableUiElement
{
    private bool currentlyBeingDragged;
    private UiElement dragArea;

    public DraggableElement(string name, Rectangle bounds, Texture2D? texture = null, Rectangle? sourceRect = null,
        Color? color = null)
        : base(name, bounds, texture, sourceRect, color)
    {
    }
}
