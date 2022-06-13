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
        private Texture2D toolButtonTexture;
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
        
        public ToolMenu(Logger l, Texture2D texture)
        {
            int startingXPos = 100;
            int startingYPos = 100;
            int startingWidth = 100;
            int startingHeight;
            toolButtonTexture = texture;

            toolButtons = new List<ToolButton>()
            {
                new ToolButton(ButtonId.Draw, 1, I18n.SmartBuilding_Buttons_Draw_Tooltip(), texture),
                new ToolButton(ButtonId.Erase, 2, I18n.SmartBuilding_Buttons_Erase_Tooltip(), texture),
                new ToolButton(ButtonId.FilledRectangle, 3, I18n.SmartBuilding_Buttons_FilledRectangle_Tooltip(), texture),
                //new ToolButton(ButtonId.Rectangle, 4, I18n.SmartBuilding_Buttons_EmptyRectangle_Tooltip(), texture),
                new ToolButton(ButtonId.Insert, 4, "Insert items into machines", texture), // Temporary. Make i18n key for this.
            };

            startingHeight = 64 * toolButtons.Count + 74;
            
            base.initialize(startingXPos, startingYPos, startingWidth, startingHeight);
            logger = l;
        }
        
        public override void draw(SpriteBatch b)
        {
            // If the menu isn't enabled, just return.
            if (!enabled)
                return;
            
            // Game1.drawDialogueBox(xPositionOnScreen, yPositionOnScreen, width, height, false, true);
            
            //Game1.DrawBox(xPositionOnScreen, yPositionOnScreen, width, height, Color.Gray);

            //b.Draw(Game1.mouseCursors, new Rectangle(xPositionOnScreen, yPositionOnScreen, width, height), new Rectangle(262, 516, 1, 1), Color.LightGray, 0f, Vector2.Zero, SpriteEffects.None, 1f);
            drawTextureBox(
                b,
                xPositionOnScreen,
                yPositionOnScreen,
                width,
                height,
                Color.LightGray
            );
            //
            // drawTextureBox(
            //     b,
            //     moveWidget.bounds,
            //     width,
            //     height,
            //     Color.Blue
            // );
            // b.Draw(Game1.mouseCursors, moveWidget.bounds, new Rectangle(262, 516, 1, 1), Color.Blue);

            // int baseTextXPos = xPositionOnScreen + 16;
            // int baseTextYPos = yPositionOnScreen + 16;
            //
            // //Game1.DrawBox(moveWidget.bounds.X, moveWidget.bounds.Y, moveWidget.bounds.Width, moveWidget.bounds.Height, Color.Red);
            // b.DrawString(Game1.dialogueFont, $"Menu xPos: {xPositionOnScreen}", new Vector2(baseTextXPos, baseTextYPos), Color.White);
            // b.DrawString(Game1.dialogueFont, $"Menu yPos: {yPositionOnScreen}", new Vector2(baseTextXPos, baseTextYPos + 32), Color.White);
            // b.DrawString(Game1.dialogueFont, $"Cursor xPos: {Game1.getMouseX()}", new Vector2(baseTextXPos, baseTextYPos + 128), Color.White);
            // b.DrawString(Game1.dialogueFont, $"Cursor yPos: {Game1.getMouseY()}", new Vector2(baseTextXPos, baseTextYPos + 160), Color.White);
            // b.DrawString(Game1.dialogueFont, $"{debugString}", new Vector2(baseTextXPos, baseTextYPos + 192), Color.White);

            //Vector2 startingPoint = new Vector2(xPositionOnScreen + 16, yPositionOnScreen + 16);
            
            foreach (ToolButton button in toolButtons)
            {
                b.Draw(toolButtonTexture, new Vector2(button.Component.bounds.X, button.Component.bounds.Y), button.Component.sourceRect, button.CurrentOverlayColour, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
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
            // Rectangle bounds = moveWidget.bounds;
            // moveWidget.bounds = new Rectangle(xPositionOnScreen, yPositionOnScreen - 64, 64, 64);
            
            Rectangle startingBounds = new Rectangle(xPositionOnScreen + 16, yPositionOnScreen + 16, 64, 64);
            
            foreach (ToolButton button in toolButtons)
            {
                button.Component.bounds = startingBounds;

                startingBounds.Y += 74;
            }
            
            foreach (ToolButton button in toolButtons)
            {
                if (ModState.ActiveTool.HasValue)
                {
                    if (button.Id.Equals(ModState.ActiveTool))
                        button.CurrentOverlayColour = Color.Red;
                    else
                        button.CurrentOverlayColour = Color.White;
                }
                else
                    button.CurrentOverlayColour = Color.White;
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
                    ModState.ActiveTool = button.Id;
                }
            }
        }

        public override void releaseLeftClick(int x, int y)
        {
            // We always want to trigger this regardless of whether or not the cursor is within the bounds, or the menu is enabled.
            
            debugString = "Left click released!";
        }

        public override void leftClickHeld(int x, int y)
        {
            
        }

        public void rightClickHeld(int x, int y)
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

            }
        }

        public override void performHoverAction(int x, int y)
        {
            // If the menu isn't enabled, just return.
            if (!enabled)
                return;

            logger.Log($"Cursor corodinates: {x}, {y}");
            
            foreach (ToolButton button in toolButtons)
            {
                logger.Log($"Button bounds: {button.Component.bounds}");
                
                if (button.Component.containsPoint(x, y))
                {
                    logger.Log($"Button {button.Index} being hovered.");
                    button.CurrentOverlayColour = Color.Gray;
                }
                else
                    button.CurrentOverlayColour = Color.White;
            }
        }
    }
}