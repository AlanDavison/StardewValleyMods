using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DecidedlyShared.Ui;

public class DraggableUiElement : ClickableUiElement
{
    private bool currentlyBeingDragged;
    private UiElement dragArea;
}
