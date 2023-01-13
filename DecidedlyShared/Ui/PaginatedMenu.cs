using System.Collections.Generic;
using System.Net.Mime;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace DecidedlyShared.Ui;

public class PaginatedMenu : ContainerElement
{
    private UiElement previousArrow;
    private UiElement nextArrow;
    private List<MenuPage> pages = new List<MenuPage>();
    private List<ContainerElement> assembledPages = new List<ContainerElement>();
    private Orientation orientation;
    private Rectangle upArrowSourceRect = new Rectangle(76, 72, 40, 44);
    private Rectangle downArrowSourceRect = new Rectangle(12, 76, 40, 44);
    private Rectangle leftArrowSourceRect = new Rectangle(8, 268, 44, 40);
    private Rectangle rightArrowSourceRect = new Rectangle(12, 204, 44, 40);
    private int currentIndex = 0;

    private int Index
    {
        get => this.currentIndex;
        set
        {
            // this.CentrePageContent();

            if (value < 0)
                this.currentIndex = 0;
            else if (value > this.pages.Count - 1)
                this.currentIndex = this.pages.Count - 1;
            else
                this.currentIndex = value;
        }
    }

    public PaginatedMenu(string name, List<MenuPage> pages, Rectangle bounds, DrawableType type = DrawableType.Texture, Texture2D? texture = null,
        Rectangle? sourceRect = null,
        Color? color = null, int topEdgeSize = 4, int bottomEdgeSize = 4, int leftEdgeSize = 4, int rightEdgeSize = 4,
        Orientation orientation = Orientation.Horizontal) :
        base(name, bounds, type, texture, sourceRect, color, topEdgeSize, bottomEdgeSize, leftEdgeSize, rightEdgeSize)
    {
        this.pages = pages;
        this.orientation = orientation;

        this.SetupPages(this.orientation);
        this.OrganiseUi(this.orientation);
    }

    private void SetupPages(Orientation orientation)
    {
        int widestPage = 0;
        int tallestPage = 0;
        this.assembledPages.Clear();

        foreach (MenuPage page in this.pages)
        {
            if (widestPage < page.TotalWidth)
                widestPage = page.TotalWidth;

            if (tallestPage < page.TotalHeight)
                tallestPage = page.TotalHeight;

            if (orientation == Orientation.Horizontal)
            {
                VBoxElement horizontalPage = new VBoxElement(
                    "Page",
                    Rectangle.Empty,
                    DrawableType.Texture
                );

                horizontalPage.AddChild(page.page);
                if (page.pageText != null)
                    horizontalPage.AddChild(page.pageText);

                this.assembledPages.Add(horizontalPage);
            }
            else
            {
                VBoxElement verticalPage = new VBoxElement(
                    "Page",
                    Rectangle.Empty,
                    DrawableType.Texture
                );

                verticalPage.AddChild(page.page);
                if (page.pageText != null)
                    verticalPage.AddChild(page.pageText);

                this.assembledPages.Add(verticalPage);
            }
        }

        // Now we set our new width and height.
        this.Width = widestPage;
        this.Height = tallestPage;

        // And centre ourselves on the screen.
        Vector2 centrePosition = Utility.getTopLeftPositionForCenteringOnScreen(this.Width, this.Height);
        this.X = (int)centrePosition.X;
        this.Y = (int)centrePosition.Y;
    }

    private void OrganiseUi(Orientation orientation)
    {
        // if (this.pages != null)
        // {
        //     foreach (UiElement page in this.pages)
        //     {
        //         Rectangle newBounds = new Rectangle(
        //             this.bounds.X,
        //             this.bounds.Y,
        //             page.bounds.Width,
        //             page.bounds.Height
        //         );
        //
        //         page.bounds = newBounds;
        //     }
        // }

        switch (orientation)
        {
            case Orientation.Horizontal:
                int horizArrowWidth = this.leftArrowSourceRect.Width;
                int horizArrowHeight = this.leftArrowSourceRect.Height;
                Rectangle leftArrowBounds = new Rectangle(
                    (int)this.BottomLeftCorner.X - horizArrowWidth, (int)this.BottomLeftCorner.Y,
                    horizArrowWidth, horizArrowHeight
                );
                Rectangle rightArrowBounds = new Rectangle(
                    (int)this.BottomRightCorner.X, (int)this.BottomRightCorner.Y,
                    horizArrowWidth, horizArrowHeight
                );

                if (this.previousArrow == null || this.nextArrow == null)
                {
                    this.previousArrow = new UiElement(
                        "Previous Arrow",
                        leftArrowBounds,
                        DrawableType.Texture,
                        Game1.mouseCursors,
                        this.leftArrowSourceRect,
                        scale: 1);
                    this.nextArrow = new UiElement(
                        "Previous Arrow",
                        rightArrowBounds,
                        DrawableType.Texture,
                        Game1.mouseCursors,
                        this.rightArrowSourceRect,
                        scale: 1);

                    this.previousArrow.leftClickCallback = this.PreviousArrowClicked;
                    this.nextArrow.leftClickCallback = this.NextArrowClicked;
                }
                else
                {
                    this.previousArrow.bounds = leftArrowBounds;
                    this.nextArrow.bounds = rightArrowBounds;
                }

                break;
            case Orientation.Vertical:
                int vertArrowWidth = this.upArrowSourceRect.Width;
                int vertArrowHeight = this.upArrowSourceRect.Height;
                Rectangle upArrowBounds = new Rectangle(
                    (int)this.TopRightCorner.X + vertArrowWidth, (int)this.TopRightCorner.Y - vertArrowHeight,
                    vertArrowWidth, vertArrowHeight
                );
                Rectangle downArrowBounds = new Rectangle(
                    (int)this.BottomRightCorner.X + vertArrowWidth, (int)this.BottomRightCorner.Y,
                    vertArrowWidth, vertArrowHeight
                );

                if (this.previousArrow == null || this.nextArrow == null)
                {
                    this.previousArrow = new UiElement(
                        "Previous Arrow",
                        upArrowBounds,
                        DrawableType.Texture,
                        Game1.mouseCursors,
                        this.upArrowSourceRect,
                        scale: 1);
                    this.nextArrow = new UiElement(
                        "Previous Arrow",
                        downArrowBounds,
                        DrawableType.Texture,
                        Game1.mouseCursors,
                        this.downArrowSourceRect,
                        scale: 1);

                    this.previousArrow.leftClickCallback = this.PreviousArrowClicked;
                    this.nextArrow.leftClickCallback = this.NextArrowClicked;
                }
                else
                {
                    this.previousArrow.bounds = upArrowBounds;
                    this.nextArrow.bounds = downArrowBounds;
                }
                break;
        }

        Vector2 centrePosition = Utility.getTopLeftPositionForCenteringOnScreen(this.Width, this.Height);
        this.X = (int)centrePosition.X;
        this.Y = (int)centrePosition.Y;
    }

    // private void CentrePageContent()
    // {
    //     Vector2 centreCoordinate = Utility.getTopLeftPositionForCenteringOnScreen(
    //         Game1.uiViewport,
    //         this.pages[this.currentIndex].bounds.Width,
    //         this.pages[this.currentIndex].bounds.Height);
    //
    //     this.pages[this.currentIndex].bounds = new Rectangle((int)centreCoordinate.X, (int)centreCoordinate.Y,
    //         this.pages[this.currentIndex].bounds.Width, this.pages[this.currentIndex].bounds.Height);
    // }

    // internal override void AddChild(UiElement child)
    // {
    //     child.bounds = new Rectangle(
    //         child.bounds.X,
    //         child.bounds.Y + this.bounds.Height,
    //         child.bounds.Width,
    //         child.bounds.Height
    //         );
    //
    //     this.bounds = new Rectangle(
    //         this.bounds.X,
    //         this.bounds.Y,
    //         this.bounds.Height + child.bounds.Height,
    //         this.bounds.Width
    //         );
    //
    //     this.OrganiseUi(this.orientation);
    //
    //     base.AddChild(child);
    // }

    private void PreviousArrowClicked()
    {
        this.Index--;
        Game1.playSound("bigSelect");
    }

    private void NextArrowClicked()
    {
        this.Index++;
        Game1.playSound("bigSelect");
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

    internal override void OrganiseChildren()
    {
        this.SetupPages(this.orientation);
        this.OrganiseUi(this.orientation);

        base.OrganiseChildren();
    }

    public override void ReceiveLeftClick(int x, int y)
    {
        if (this.previousArrow.bounds.Contains(x, y))
            this.previousArrow.ReceiveLeftClick(x, y);

        if (this.nextArrow.bounds.Contains(x, y))
            this.nextArrow.ReceiveLeftClick(x, y);

        base.ReceiveLeftClick(x, y);
    }

    internal override void Draw(SpriteBatch spriteBatch)
    {
        if (this.currentIndex == 0)
            this.previousArrow.Draw(spriteBatch, Color.DarkGray);
        else
            this.previousArrow.Draw(spriteBatch);

        if (this.currentIndex == this.pages.Count)
            this.nextArrow.Draw(spriteBatch, Color.DarkGray);
        else
            this.nextArrow.Draw(spriteBatch);

        if (this.pages.Count >= 1)
            this.pages[this.currentIndex].Draw(spriteBatch);

        base.Draw(spriteBatch);
    }
}
