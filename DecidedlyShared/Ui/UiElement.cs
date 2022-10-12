using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Menus;

namespace DecidedlyShared.Ui;

public abstract class UiElement
{
    internal Rectangle bounds;
    internal Texture2D texture;
    internal Rectangle sourceRect;
    internal Color textureTint;
    internal string elementName;

    public void Draw(SpriteBatch sb)
    {
        sb.Draw(this.texture, this.bounds, this.sourceRect, this.textureTint);
    }
}
