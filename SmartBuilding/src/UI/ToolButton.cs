using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SmartBuilding.Utilities;
using StardewValley;
using StardewValley.Menus;

namespace SmartBuilding.UI
{
    public class ToolButton
    {
        private ClickableTextureComponent buttonComponent;
        private ButtonId buttonId;
        private Color currentOverlayColour;
        private int buttonIndex;
        private string buttonTooltip;

        public ClickableTextureComponent Component
        {
            get => buttonComponent;
        }

        public Color CurrentOverlayColour
        {
            get => currentOverlayColour;
            set { currentOverlayColour = value; }
        }
        
        public int Index
        {
            get => buttonIndex;
        }

        public ButtonId Id
        {
            get => buttonId;
        }

        /// <summary>
        /// Everything can be derived from the button ID.
        /// </summary>
        /// <param name="button"></param>
        public ToolButton(ButtonId button, int index, string tooltip, Texture2D texture)

        {
            Rectangle sourceRect = Ui.GetButtonSourceRect(button);
            buttonIndex = index;
            buttonTooltip = tooltip;
            buttonId = button;

            buttonComponent = new ClickableTextureComponent(
                new Rectangle(0, 0, 0, 0),
                texture,
                sourceRect,
                1f
            );
        }
    }
}