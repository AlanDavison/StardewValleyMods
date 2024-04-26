using System;
using System.Collections.Generic;
using System.Linq;
using DecidedlyShared.Logging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace SmartBuilding.UI
{
    public class ToolMenu : IClickableMenu
    {
        private readonly ModState modState;
        private readonly List<ToolButton> toolButtons;
        private readonly Texture2D windowSkin;
        private int currentMouseX = 0;
        private int currentMouseY = 0;
        private ModConfig config;

        // Debug gubbins.
        private string debugString;

        // Input state gubbins
        private bool leftMouseDown;

        private Logger logger;
        private int previousMouseX;
        private int previousMouseY;
        private Texture2D toolButtonSpritesheet;
        private bool windowBeingDragged;

        public ToolMenu(Logger l, Texture2D buttonSpritesheet, List<ToolButton> buttons, ModState modState, ModConfig config)
        {
            int startingXPos = (int)MathF.Round(100 * Game1.options.uiScale);
            int startingYPos = (int)MathF.Round(100 * Game1.options.uiScale);
            int startingWidth = 100;
            int startingHeight = 0;
            this.modState = modState;
            this.toolButtonSpritesheet = buttonSpritesheet;
            this.windowSkin = this.windowSkin;
            this.toolButtons = buttons;
            this.config = config;

            // First, we increment our height by 64 for every button, unless it's a layer button.
            foreach (var button in this.toolButtons)
            {
                if (button.Type != ButtonType.Layer)
                    startingHeight += 64;
            }

            // Then, we add 8 per button to allow 8 pixels of spacing between buttons.
            startingHeight += this.toolButtons.Count * 8;

            this.initialize(startingXPos, startingYPos, startingWidth, startingHeight);
            this.logger = l;

            this.modState.ActiveTool = ButtonId.Draw;
        }

        public bool Enabled { get; set; } = false;

        public override void draw(SpriteBatch b)
        {
            // If the menu isn't enabled, just return.
            if (!this.Enabled)
                return;

            if (this.modState.ActiveTool != ButtonId.None)
            {
                if (this.modState.ActiveTool == ButtonId.Erase)
                {
                    DecidedlyShared.Ui.Utils.DrawBox(
                        b,
                        Game1.menuTexture,
                        new Rectangle(0, 256, 60, 60),
                        this.xPositionOnScreen + 64, this.yPositionOnScreen,
                        this.width + 32 + 8,
                        64 * 4 + 8 * 8 + 64,
                        16, 12, 16, 12
                    );

                    DecidedlyShared.Ui.Utils.DrawBox(
                        b,
                        Game1.menuTexture,
                        new Rectangle(0, 256, 60, 60),
                        this.xPositionOnScreen + 64, this.yPositionOnScreen,
                        this.width + 32 + 8,
                        64 * 4 + 8 * 8 + 64,
                        16, 12, 16, 12
                    );
                }
            }

            DecidedlyShared.Ui.Utils.DrawBox(
                b,
                Game1.menuTexture,
                new Rectangle(0, 256, 60, 60),
                this.xPositionOnScreen, this.yPositionOnScreen,
                this.width, this.height,
                16, 12, 16, 12
            );

            foreach (var button in this.toolButtons) button.Draw(b);

            foreach (var button in this.toolButtons)
            {
                if (button.IsHovered)
                {
                    Utility.drawTextWithColoredShadow(b, button.ButtonTooltip, Game1.dialogueFont,
                        new Vector2(Game1.getMouseX() + 78, Game1.getMouseY()),
                        Color.WhiteSmoke, new Color(Color.Black, 0.75f));
                }
            }

            this.drawMouse(b);
            base.draw(b);
        }

        public override void update(GameTime time)
        {
            // If the menu isn't enabled, just return.
            if (!this.Enabled)
                return;

            this.UpdateComponents();
        }

        private void DoWindowDrag(int x, int y)
        {
            if (!this.Enabled || !this.windowBeingDragged)
                return;
        }

        private void UpdateComponents()
        {
            // If the menu isn't enabled, just return.
            if (!this.Enabled)
                return;

            var startingBounds = new Rectangle(this.xPositionOnScreen + 16, this.yPositionOnScreen + 16, 64, 64);

            foreach (var button in this.toolButtons)
            {
                button.Component.bounds = startingBounds;

                startingBounds.Y += 64 + 8;
                // startingBounds.Y += 74;
            }

            foreach (var button in this.toolButtons)
            {
                if (button.Type == ButtonType.Layer)
                {
                    button.Component.bounds = new Rectangle(button.Component.bounds.X + 64 + 32,
                        button.Component.bounds.Y - 430,
                        button.Component.bounds.Width,
                        button.Component.bounds.Height);

                    if (button.LayerToTarget == TileFeature.Furniture)
                        button.Component.bounds.Height = 128;
                }
            }

            // if (this.modState.ActiveTool == ButtonId.Erase && this.modState.SelectedLayer == TileFeature.None)
            //     this.modState.SelectedLayer = TileFeature.Drawn;
        }

        public void ReceiveLeftClick(int x, int y)
        {
            // If the menu isn't enabled, just return.
            if (!this.Enabled)
                return;

            // This is where we'll loop through all of our buttons, and perform actions appropriately.
            foreach (var button in this.toolButtons)
            {
                if (button.Component.containsPoint(x, y))
                {
                    if (button.Type == ButtonType.Layer)
                    {
                        if (this.modState.ActiveTool != ButtonId.None)
                            if (this.modState.ActiveTool == ButtonId.Erase)
                                button.ButtonAction();
                    }
                    else
                    {
                        this.modState.SelectedLayer = TileFeature.None;
                        button.ButtonAction();
                    }
                }
            }
        }

        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            this.LockWithinBounds(ref this.xPositionOnScreen, ref this.yPositionOnScreen);
        }

        public void MiddleMouseReleased(int x, int y)
        {
            if (!this.Enabled)
                return;

            this.windowBeingDragged = false;
        }

        public void MiddleMouseHeld(int x, int y)
        {
            // If the menu isn't enabled, just return.
            if (!this.Enabled)
                return;

            // This is where we'll handle moving the UI. It doesn't matter which element the cursor is over.
            if (this.isWithinBounds(x, y) || this.windowBeingDragged)
            {
                this.windowBeingDragged = true;

                var newBounds = new Rectangle(this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height);

                int xDelta = x - this.previousMouseX;
                int yDelta = y - this.previousMouseY;

                this.xPositionOnScreen += xDelta;
                this.yPositionOnScreen += yDelta;
                this.LockWithinBounds(ref this.xPositionOnScreen, ref this.yPositionOnScreen);
            }
        }

        public void ReceiveGamePadButton(SButton b)
        {
            ButtonId currentButton = this.modState.ActiveTool;

            switch (currentButton)
            {
                case ButtonId.Draw:
                    if (b == SButton.DPadDown)
                        this.modState.ActiveTool = ButtonId.Erase;
                    break;
                case ButtonId.Erase:
                    if (this.modState.SelectedLayer == TileFeature.None)
                    {
                        if (b == SButton.DPadDown)
                        {
                            this.modState.ActiveTool = ButtonId.FilledRectangle;
                            break;
                        }
                        if (b == SButton.DPadUp)
                        {
                            this.modState.ActiveTool = ButtonId.Draw;
                            break;
                        }
                        if (b == SButton.DPadRight)
                        {
                            this.modState.SelectedLayer = TileFeature.Drawn;
                        }
                    }

                    switch (this.modState.SelectedLayer)
                    {
                        case TileFeature.Drawn:
                            if (b == SButton.DPadDown)
                                this.modState.SelectedLayer = TileFeature.Object;
                            if (b == SButton.DPadLeft)
                            {
                                this.modState.ActiveTool = ButtonId.Erase;
                                this.modState.SelectedLayer = TileFeature.None;
                            }
                            break;
                        case TileFeature.Object:
                            if (b == SButton.DPadDown)
                                this.modState.SelectedLayer = TileFeature.TerrainFeature;
                            if (b == SButton.DPadUp)
                                this.modState.SelectedLayer = TileFeature.Drawn;
                            if (b == SButton.DPadLeft)
                            {
                                this.modState.ActiveTool = ButtonId.Erase;
                                this.modState.SelectedLayer = TileFeature.None;
                            }
                            break;
                        case TileFeature.TerrainFeature:
                            if (b == SButton.DPadDown)
                                this.modState.SelectedLayer = TileFeature.Furniture;
                            if (b == SButton.DPadUp)
                                this.modState.SelectedLayer = TileFeature.Object;
                            if (b == SButton.DPadLeft)
                            {
                                this.modState.ActiveTool = ButtonId.Erase;
                                this.modState.SelectedLayer = TileFeature.None;
                            }
                            break;
                        case TileFeature.Furniture:
                            if (b == SButton.DPadUp)
                                this.modState.SelectedLayer = TileFeature.TerrainFeature;
                            if (b == SButton.DPadLeft)
                            {
                                this.modState.ActiveTool = ButtonId.Erase;
                                this.modState.SelectedLayer = TileFeature.None;
                            }
                            break;
                    }
                    break;
                case ButtonId.FilledRectangle:
                    if (b == SButton.DPadDown)
                        this.modState.ActiveTool = ButtonId.Insert;
                    if (b == SButton.DPadUp)
                        this.modState.ActiveTool = ButtonId.Erase;
                    break;
                case ButtonId.Insert:
                    if (b == SButton.DPadDown)
                        this.modState.ActiveTool = ButtonId.ConfirmBuild;
                    if (b == SButton.DPadUp)
                        this.modState.ActiveTool = ButtonId.FilledRectangle;
                    break;
                case ButtonId.ConfirmBuild:
                    if (b == SButton.DPadDown)
                        this.modState.ActiveTool = ButtonId.ClearBuild;
                    if (b == SButton.DPadUp)
                        this.modState.ActiveTool = ButtonId.Insert;
                    if (b == this.config.PressButton)
                        this.toolButtons.First(b => b.Id == ButtonId.ConfirmBuild).ButtonAction();
                    break;
                case ButtonId.ClearBuild:
                    if (b == SButton.DPadUp)
                        this.modState.ActiveTool = ButtonId.ConfirmBuild;
                    if (b == this.config.PressButton)
                        this.toolButtons.First(b => b.Id == ButtonId.ClearBuild).ButtonAction();
                    break;
                case ButtonId.None:
                    break;
            }
        }

        private void LockWithinBounds(ref int x, ref int y)
        {
            // First, we check to see if the window is out of bounds to the left or above.
            if (x < 0)
                x = 0;
            if (y < 0)
                y = 0;

            // Then we check in the positive (to the right and down).
            if (x + this.width + 110 > Game1.uiViewport.Width)
                x = Game1.uiViewport.Width - this.width - 110;
            if (y + this.height > Game1.uiViewport.Height)
                y = Game1.uiViewport.Height - this.height;
        }

        public void SetCursorHoverState(int x, int y)
        {
            this.modState.BlockMouseInteractions = this.isWithinBounds(x, y);
        }

        public override bool isWithinBounds(int x, int y)
        {
            bool isInMainWindowBounds = base.isWithinBounds(x, y);

            foreach (var button in this.toolButtons)
            {
                if (button.IsHovered)
                    return true;
            }

            return isInMainWindowBounds;
        }

        public void DoHover(int x, int y)
        {
            // If the menu isn't enabled, just return.
            if (!this.Enabled)
                return;

            foreach (var button in this.toolButtons)
            {
                if (button.Component.containsPoint(x, y))
                {
                    // If it's a layer button, we only want to do anything if erase is the currently selected tool.
                    if (button.Type == ButtonType.Layer)
                    {
                        if (this.modState.ActiveTool != ButtonId.None && this.modState.ActiveTool == ButtonId.Erase)
                        {
                            button.CurrentOverlayColour = Color.Gray;
                            button.IsHovered = true;
                        }
                    }
                    else
                    {
                        button.CurrentOverlayColour = Color.Gray;
                        button.IsHovered = true;
                    }
                }
                else
                {
                    button.CurrentOverlayColour = Color.White;
                    button.IsHovered = false;
                }
            }

            this.previousMouseX = x;
            this.previousMouseY = y;
        }

        public override void performHoverAction(int x, int y)
        {
        }
    }
}
