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
    private string checkboxLabel;
    private TfrFeature associatedFeature;

    public TfrCheckbox(Rectangle bounds, string name, ref TfrFeature feature) : base(bounds, name)
    {
        this.checkboxImage = Game1.menuTexture;
        this.checkedSourceRect = new Rectangle(192, 768, 36, 36);
        this.uncheckedSourceRect = new Rectangle(128, 768, 36, 36);

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

        Utility.drawTextWithShadow(
            sb,
            this.name,
            Game1.smallFont,
            new Vector2(this.bounds.X + 36 + 4, this.bounds.Y + 4),
            Game1.textColor);
    }

    public void ReceiveLeftClick()
    {
        if (this.isChecked)
        {
            this.isChecked = false;
            this.associatedFeature.actionToTake = TfrAction.None;
        }
        else
            this.isChecked = true;
    }

    public override bool containsPoint(int x, int y)
    {
        return base.containsPoint(x, y);
    }
}
