using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace DecidedlyShared.Ui;

public class PaginatedMenu : UiElement
{
    private ClickableUiElement previousArrow;
    private ClickableUiElement nextArrow;
    private List<ContainerElement> pages;
    private Orientation orientation;
    private Rectangle upArrowSourceRect = new Rectangle(76, 72, 40, 44);
    private Rectangle downArrowSourceRect = new Rectangle(12, 76, 40, 44);
    private Rectangle leftArrowSourceRect = new Rectangle(8, 268, 44, 40);
    private Rectangle rightArrowSourceRect = new Rectangle(12, 204, 44, 40);

    public PaginatedMenu(string name, List<ContainerElement> pages, Rectangle bounds, Texture2D? texture = null,
        Rectangle? sourceRect = null,
        Color? color = null, int topEdgeSize = 4, int bottomEdgeSize = 4, int leftEdgeSize = 4, int rightEdgeSize = 4,
        Orientation orientation = Orientation.Horizontal) :
        base(name, bounds, texture, sourceRect, color, topEdgeSize, bottomEdgeSize, leftEdgeSize, rightEdgeSize)
    {
        this.pages = pages;
        this.orientation = orientation;

        switch (orientation)
        {
            case Orientation.Horizontal:
                this.previousArrow = new ClickableUiElement("Left Arrow",
                    this.leftArrowSourceRect, Game1.mouseCursors, this.leftArrowSourceRect);
                this.nextArrow = new ClickableUiElement("Right Arrow",
                    this.rightArrowSourceRect, Game1.mouseCursors, this.rightArrowSourceRect);
                break;
            case Orientation.Vertical:
                this.previousArrow = new ClickableUiElement("Up Arrow",
                    this.upArrowSourceRect, Game1.mouseCursors, this.upArrowSourceRect);
                this.nextArrow = new ClickableUiElement("Down Arrow",
                    this.downArrowSourceRect, Game1.mouseCursors, this.downArrowSourceRect);
                break;
        }
    }

    // public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
    // {
    //     switch (this.orientation)
    //     {
    //         case Orientation.Horizontal:
    //             this.previousArrow.bounds.X = this.uiContainer.bounds.Left;
    //             this.previousArrow.bounds.Y = this.uiContainer.bounds.Bottom + 40 + 16;
    //
    //             this.nextArrow.bounds.X = this.uiContainer.bounds.Right - 40;
    //             this.nextArrow.bounds.Y = this.uiContainer.bounds.Bottom + 40 + 16;
    //             break;
    //         case Orientation.Vertical:
    //             this.previousArrow.bounds.X = this.uiContainer.bounds.Right + 44 + 16;
    //             this.previousArrow.bounds.Y = this.uiContainer.bounds.Top;
    //
    //             this.nextArrow.bounds.X = this.uiContainer.bounds.Right + 44 + 16;
    //             this.nextArrow.bounds.Y = this.uiContainer.bounds.Bottom - 44;
    //             break;
    //     }
    //
    //     base.gameWindowSizeChanged(oldBounds, newBounds);
    // }
    //
    // public override void draw(SpriteBatch b)
    // {
    //     this.previousArrow.Draw(b);
    //     this.nextArrow.Draw(b);
    //
    //     base.draw(b);
    // }
}
