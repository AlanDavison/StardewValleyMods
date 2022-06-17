using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using SmartBuilding.Utilities;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace SmartBuilding.UI
{
    public class ToolMenu : IClickableMenu
    {
        private List<ToolButton> toolButtons;
        private Texture2D toolButtonSpritesheet;
        private Texture2D windowSkin;
        private bool enabled = false;

        private int previousMouseX = 0;
        private int previousMouseY = 0;

        private Logger logger;

        // Input state gubbins
        private bool leftMouseDown;

        // Debug gubbins.
        private string debugString;

        public bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }

        public ToolMenu(Logger l, Texture2D buttonSpritesheet, Texture2D windowSkin, List<ToolButton> buttons)
        {
            int startingXPos = (int)MathF.Round(100 * Game1.options.uiScale);
            int startingYPos = (int)MathF.Round(100 * Game1.options.uiScale);
            int startingWidth = 100;
            int startingHeight = 0;
            toolButtonSpritesheet = buttonSpritesheet;
            this.windowSkin = windowSkin;
            toolButtons = buttons;

            // startingHeight = 64 * (toolButtons.Count + 1)

            startingHeight += 4 * 4;

            foreach (ToolButton button in toolButtons)
            {
                startingHeight += 64 + 8;
            }

            startingHeight += 4 * 4;

            base.initialize(startingXPos, startingYPos, startingWidth, startingHeight);
            logger = l;
        }

        public override void draw(SpriteBatch b)
        {
            // If the menu isn't enabled, just return.
            if (!enabled)
                return;

            drawTextureBox(
                b,
                xPositionOnScreen,
                yPositionOnScreen,
                width,
                height,
                Color.LightGray
            );

            drawTextureBox(
                b,
                Game1.menuTexture,
                new Rectangle(0, 256, 60, 60),
                xPositionOnScreen,
                yPositionOnScreen + (8 * 3) + 64 * 3,
                width,
                240,
                Color.White,
                1f,
                false
            );

            foreach (ToolButton button in toolButtons)
            {
                button.Draw(b);

                //b.Draw(toolButtonTexture, new Vector2(button.Component.bounds.X, button.Component.bounds.Y), button.Component.sourceRect, button.CurrentOverlayColour, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
                if (button.IsHovered)
                {
                    // b.DrawString(Game1.smallFont, button.ButtonTooltip, new Vector2(Game1.getMouseX() + 79, Game1.getMouseY() + 1), Color.Black);
                    Utility.drawTextWithColoredShadow(b, button.ButtonTooltip, Game1.dialogueFont, new Vector2(Game1.getMouseX() + 78, Game1.getMouseY()), Color.WhiteSmoke, new Color(Color.Black, 0.75f));
                    //drawToolTip(b, button.ButtonTooltip, "Title", null, new Vector2(Game1.getMouseX() + 78, Game1.getMouseY()), Color.White);
                }
            }

            drawMouse(b);
            base.draw(b);
        }

        public override void update(GameTime time)
        {
            // If the menu isn't enabled, just return.
            if (!enabled)
                return;

            UpdateComponents();
        }

        private void UpdateComponents()
        {
            // If the menu isn't enabled, just return.
            if (!enabled)
                return;

            Rectangle startingBounds = new Rectangle(xPositionOnScreen + 16, yPositionOnScreen + 16, 64, 64);

            foreach (ToolButton button in toolButtons)
            {
                button.Component.bounds = startingBounds;

                startingBounds.Y += 64 + 8;
                // startingBounds.Y += 74;
            }

            foreach (ToolButton button in toolButtons)
            {
                // if (button.Type == ButtonType.Tool)
                // {
                //     if (ModState.ActiveTool.HasValue)
                //     {
                //         if (button.Id.Equals(ModState.ActiveTool))
                //             button.CurrentOverlayColour = Color.Red;
                //         else
                //             button.CurrentOverlayColour = Color.White;
                //     }
                //     else
                //         button.CurrentOverlayColour = Color.White;
                // }
            }
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            // If the menu isn't enabled, just return.
            if (!enabled)
                return;

            // This is where we'll look through all of our buttons, and perform actions appropriately.
            foreach (ToolButton button in toolButtons)
            {
                if (button.Component.containsPoint(x, y))
                {
                    if (button.Type == ButtonType.Layer)
                    {
                        if (ModState.ActiveTool.HasValue)
                        {
                            if (ModState.ActiveTool.Value == ButtonId.Erase)
                                button.ButtonAction();
                        }
                    }
                    else
                    {
                        ModState.SelectedLayer = null;
                        button.ButtonAction();
                    }
                    // // First, we check to see if a button is a too
                    // if (button.Type == ButtonType.Tool)
                    //     ModState.ActiveTool = button.Id;
                    //
                    // if (button.Type == ButtonType.Function)
                    // {
                    //     if (button.Id == ButtonId.ConfirmBuild)
                    //         confirmBuild();
                    //     
                    //     if (button.Id == ButtonId.ClearBuild)
                    //         clearBuild();
                    // }
                }
            }
        }

        public void middleMouseHeld(int x, int y)
        {
            // If the menu isn't enabled, just return.
            if (!enabled)
                return;

            // This is where we'll handle moving the UI. It doesn't matter which element the cursor is over.
            if (this.isWithinBounds(x, y))
            {
                Rectangle newBounds = new Rectangle(xPositionOnScreen, yPositionOnScreen, width, height);

                xPositionOnScreen = x - newBounds.Width / 2;
                yPositionOnScreen = (int)Math.Round(y - newBounds.Width * 0.5f);

                // xPositionOnScreen = (int)MathF.Round(xPositionOnScreen * Game1.options.uiScale);
                // yPositionOnScreen = (int)MathF.Round(yPositionOnScreen * Game1.options.uiScale);
            }
        }

        public void SetCursorHoverState(int x, int y)
        {
            ModState.BlockMouseInteractions = isWithinBounds(x, y);
        }

        public override bool isWithinBounds(int x, int y)
        {
            bool isInBounds = base.isWithinBounds(x, y);

            if (!isInBounds)
            {
                foreach (ToolButton button in toolButtons)
                {
                    button.IsHovered = false;
                    button.CurrentOverlayColour = Color.White;
                }
            }

            return isInBounds;
        }

        public override void performHoverAction(int x, int y)
        {
            // If the menu isn't enabled, just return.
            if (!enabled)
                return;

            ;

            foreach (ToolButton button in toolButtons)
            {
                if (button.Component.containsPoint(x, y))
                {
                    button.CurrentOverlayColour = Color.Gray;
                    button.IsHovered = true;
                }
                else
                {
                    button.CurrentOverlayColour = Color.White;
                    button.IsHovered = false;
                }
            }
        }
    }
}