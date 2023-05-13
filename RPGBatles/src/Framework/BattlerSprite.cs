using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace RPGBattles.Framework;

public abstract class BattlerSprite
{
    private AnimatedSprite sprite;
    private int xPos, yPos;

    public int X
    {
        get => this.xPos;
        set { this.xPos = value; }
    }

    public int Y
    {
        get => this.yPos;
        set { this.yPos = value; }
    }

    public BattlerSprite(AnimatedSprite sprite)
    {
        this.sprite = sprite;
    }

    public void Draw(SpriteBatch sb)
    {
        this.sprite.draw(sb, new Vector2(this.xPos, this.yPos), 1f);
    }
}
