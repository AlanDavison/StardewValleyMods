using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace DecidedlyShared.Ui;

public abstract class VBoxUiElement : UiElement
{
    private List<UiElement> childElements;
    internal bool resizeToFitElements;
    internal int maximumHeight;

    public virtual void Draw(SpriteBatch sb)
    {
        base.Draw(sb);

        foreach (UiElement child in this.childElements)
        {

        }
    }

}
