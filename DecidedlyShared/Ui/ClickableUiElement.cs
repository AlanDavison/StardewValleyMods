using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DecidedlyShared.Ui;

public class ClickableUiElement : UiElement
{
    private Action? leftClickAction;
    private Action? rightClickAction;
}
