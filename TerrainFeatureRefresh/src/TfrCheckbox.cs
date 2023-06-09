using DecidedlyShared.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using TerrainFeatureRefresh.Framework;

namespace TerrainFeatureRefresh;

public class TfrCheckbox : ClickableComponent
{
    private Texture2D checkboxImage;
    private Rectangle checkedSourceRect;
    private Rectangle uncheckedSourceRect;
    private bool isChecked;
    private TfrFeature associatedFeature;

    public TfrCheckbox(Rectangle bounds, string name, Texture2D texture, ref TfrFeature feature) : base(bounds, name)
    {
        this.checkboxImage = texture;
        this.uncheckedSourceRect = new Rectangle(0, 0, 47, 47);
        this.checkedSourceRect = new Rectangle(52, 0, 47, 47);

        Vector2 labelBounds = Game1.smallFont.MeasureString(name);

        this.bounds.Width = (int)labelBounds.X + 40;
        this.bounds.Height = (int)labelBounds.Y;

        this.associatedFeature = feature;
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

    public void ReceiveLeftClick()
    {
        if (this.isChecked)
        {
            this.isChecked = false;
            this.associatedFeature.actionToTake = TfrAction.None;
        }
        else
        {
            this.isChecked = true;
            this.associatedFeature.actionToTake = TfrAction.Regenerate;
        }
    }

    public override bool containsPoint(int x, int y)
    {
        return base.containsPoint(x, y);
    }
}
