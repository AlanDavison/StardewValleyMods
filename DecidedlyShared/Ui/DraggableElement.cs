using DecidedlyShared.Logging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DecidedlyShared.Ui;

public class DraggableElement : UiElement
{
    private bool currentlyBeingDragged;
    private UiElement dragArea;

    public DraggableElement(string name, Rectangle bounds, Logger logger, DrawableType type = DrawableType.Texture,Texture2D? texture = null, Rectangle? sourceRect = null,
        Color? color = null)
        : base(name, bounds, logger, type, texture, sourceRect, color)
    {
    }
}
