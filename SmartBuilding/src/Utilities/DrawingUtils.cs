using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SmartBuilding.Utilities
{
    public class DrawingUtils
    {
        public DrawingUtils() { }

        public void DrawStringWithShadow(SpriteBatch spriteBatch, SpriteFont font, string text, Vector2 position, Color textColour, Color shadowColour)
        {
            spriteBatch.DrawString(
                spriteFont: font,
                text: text,
                position: position + new Vector2(2, 2),
                shadowColour
            );

            spriteBatch.DrawString(
                spriteFont: font,
                text: text,
                position: position,
                textColour
            );
        }
    }
}