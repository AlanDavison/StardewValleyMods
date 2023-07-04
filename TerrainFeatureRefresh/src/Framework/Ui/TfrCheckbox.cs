using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TerrainFeatureRefresh.Framework.Ui;

public class TfrCheckbox : Checkbox
{
    private TfrFeature associatedFeature;

    public TfrCheckbox(Rectangle bounds, string name, Texture2D texture, ref TfrFeature feature) : base(bounds, name, texture)
    {
        this.associatedFeature = feature;
    }

    public override void ReceiveLeftClick()
    {
        base.ReceiveLeftClick();

        if (this.isChecked)
        {
            this.associatedFeature.actionToTake = TfrAction.Process;
        }
        else
        {
            this.associatedFeature.actionToTake = TfrAction.Ignore;
        }
    }
}
