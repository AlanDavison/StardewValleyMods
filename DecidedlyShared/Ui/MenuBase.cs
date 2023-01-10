using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace DecidedlyShared.Ui;

public class MenuBase : IClickableMenu
{
    // Every base menu needs a container, and only a container.
    internal ContainerElement uiContainer;
    private string menuName;
    private string openSound;

    public string MenuName
    {
        get => this.menuName;
    }

    public Vector2 TopLeftCorner
    {
        get => new Vector2(this.uiContainer.bounds.Left, this.uiContainer.bounds.Top);
    }

    public Vector2 BottomLeftCorner
    {
        get => new Vector2(this.uiContainer.bounds.Left, this.uiContainer.bounds.Bottom);
    }

    public Vector2 TopRightCorner
    {
        get => new Vector2(this.uiContainer.bounds.Right, this.uiContainer.bounds.Top);
    }

    public Vector2 BottomRightCorner
    {
        get => new Vector2(this.uiContainer.bounds.Right, this.uiContainer.bounds.Bottom);
    }

    public MenuBase(ContainerElement uiContainer, string name, string openSound = "bigSelect")
    {
        this.uiContainer = uiContainer;
        this.xPositionOnScreen = 0;
        this.yPositionOnScreen = 0;
        this.width = Game1.uiViewport.Width;
        this.height = Game1.uiViewport.Height;
        this.uiContainer.textureTint = Color.White;
        this.menuName = name;
        this.openSound = openSound;
    }

    public void MenuOpened()
    {
        Game1.playSound(this.openSound);
    }

    public override void draw(SpriteBatch b)
    {
        this.uiContainer.Draw(b);
        this.drawMouse(b);
    }

    public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
    {
        this.xPositionOnScreen = 0;
        this.yPositionOnScreen = 0;
        this.width = Game1.uiViewport.Width;
        this.height = Game1.uiViewport.Height;
        this.uiContainer.OrganiseChildren();
    }

    public override void emergencyShutDown()
    {
        base.emergencyShutDown();
    }
}
