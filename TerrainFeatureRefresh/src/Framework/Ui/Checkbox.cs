using DecidedlyShared.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace TerrainFeatureRefresh.Framework.Ui;

public class Checkbox : ClickableComponent
{
    internal Texture2D checkboxImage;
    internal Rectangle checkedSourceRect;
    internal Rectangle uncheckedSourceRect;
    internal bool isChecked;

    public Checkbox(Rectangle bounds, string name, Texture2D texture) : base(bounds, name)
    {
        this.checkboxImage = texture;
        this.uncheckedSourceRect = new Rectangle(0, 0, 47, 47);
        this.checkedSourceRect = new Rectangle(52, 0, 47, 47);

        Vector2 labelBounds = Game1.smallFont.MeasureString(name);

        this.bounds.Width = (int)labelBounds.X + 40;
        this.bounds.Height = (int)labelBounds.Y;
    }

    public void Draw(SpriteBatch sb)
    {
        sb.Draw(
            this.checkboxImage,
            new Rectangle(this.bounds.X, this.bounds.Y, 36, 36),
            this.isChecked ? this.checkedSourceRect : this.uncheckedSourceRect,
            Color.White);

        Drawing.DrawStringWithShadow(
            sb,
            Game1.smallFont,
            this.name,
            new Vector2(this.bounds.X + 36 + 4, this.bounds.Y + 4),
            Color.Black,
            Color.Gray);

        // Utility.drawTextWithShadow(
        //     sb,
        //     this.name,
        //     Game1.smallFont,
        //     new Vector2(this.bounds.X + 36 + 4, this.bounds.Y + 4),
        //     Game1.textColor);
    }

    public virtual void ReceiveLeftClick()
    {
        this.isChecked = !this.isChecked;
    }

    public override bool containsPoint(int x, int y)
    {
        return base.containsPoint(x, y);
    }
}
