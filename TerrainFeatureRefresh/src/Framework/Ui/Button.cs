using System;
using DecidedlyShared.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace TerrainFeatureRefresh.Framework.Ui;

public class Button : ClickableComponent
{
    private Rectangle sourceRect;
    private Texture2D texture;
    private string buttonLabel;
    private bool isHovered;
    private Action? clickAction;

    public Button(Rectangle bounds, string name, Texture2D texture, Rectangle sourceRect) : base(bounds, name)
    {
        this.buttonLabel = name;
        int textWidth = (int)Game1.smallFont.MeasureString(this.buttonLabel).X;
        this.bounds = new Rectangle(0, 0, textWidth + 32, 32 + 16);
        this.texture = texture;
        this.sourceRect = sourceRect;
    }

    public void Draw(SpriteBatch sb)
    {
        Color color = this.isHovered ? new Color(240, 240, 240) : new Color(255, 180, 110);
        // Color color = this.isHovered ? new Color(255, 255, 122) : new Color(255, 255, 255);

        DecidedlyShared.Ui.Utils.DrawBox(
            sb,
            this.texture,
            this.sourceRect,
            this.bounds,
            4,
            4,
            8,
            8);

        Drawing.DrawStringWithShadow(
            sb,
            Game1.smallFont,
            this.buttonLabel,
            new Vector2(this.bounds.X + 16, this.bounds.Y + 10),
            Color.Black,
            Color.Gray);

        // Utility.drawTextWithShadow(
        //     sb,
        //     this.buttonLabel,
        //     Game1.smallFont,
        //     new Vector2(this.bounds.X + 16, this.bounds.Y + 10),
        //     Game1.textColor);
    }

    public Button(Rectangle bounds, string name, string label) : base(bounds, name, label)
    {
    }

    public Button(Rectangle bounds, Item item) : base(bounds, item)
    {
    }

    public void ReceiveLeftClick()
    {

    }

    public void DoHover(int x, int y)
    {
        if (this.bounds.Contains(x, y))
            this.isHovered = true;
        else
            this.isHovered = false;
    }
}
