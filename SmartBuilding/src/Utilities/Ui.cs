using Microsoft.Xna.Framework;
using SmartBuilding.UI;

namespace SmartBuilding.Utilities
{
    public class Ui
    {
        /// <summary>
        /// Get the source rectangle for the appropriate Texture2D for each individual button.
        /// </summary>
        /// <param name="button">The ID of the button being checked.</param>
        /// <returns></returns>
        public static Rectangle GetButtonSourceRect(ButtonId button)
        {
            switch (button)
            {
                case ButtonId.Draw:
                    return new Rectangle(0, 0, 16, 16);
                case ButtonId.Erase:
                    return new Rectangle(16, 0, 16, 16);
                case ButtonId.Insert:
                    return new Rectangle(32, 0, 16, 16);
                case ButtonId.Rectangle:
                    return new Rectangle(16, 16, 16, 16);
                case ButtonId.FilledRectangle:
                    return new Rectangle(0, 16, 16, 16);
                case ButtonId.Select:
                    return new Rectangle(32, 16, 16, 16);
            }

            return new Rectangle(1, 3, 3, 7);
        }
    }
}