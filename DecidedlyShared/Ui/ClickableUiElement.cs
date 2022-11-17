using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DecidedlyShared.Ui;

public class ClickableUiElement : UiElement
{
    private Action? leftClickAction;
    private Action? rightClickAction;

    public ClickableUiElement(string name, Rectangle bounds, Texture2D? texture = null, Rectangle? sourceRect = null,
        Color? color = null)
        : base(name, bounds, texture, sourceRect, color)
    {
    }
}
