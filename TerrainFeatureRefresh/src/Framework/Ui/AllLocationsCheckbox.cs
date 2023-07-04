using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TerrainFeatureRefresh.Framework.Ui;

public class AllLocationsCheckbox : Checkbox
{
    private TfrToggle toggle;

    public AllLocationsCheckbox(Rectangle bounds, string name, Texture2D texture, ref TfrToggle toggle) : base(bounds, name, texture)
    {
        this.toggle = toggle;
    }

    public override void ReceiveLeftClick()
    {
        base.ReceiveLeftClick();

        this.toggle.On = this.isChecked;
    }
}
