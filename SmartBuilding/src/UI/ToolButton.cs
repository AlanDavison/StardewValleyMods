using System;
using System.Net.WebSockets;
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
        private ButtonType buttonType;
        private Action buttonAction;
        private TileFeature? layerToTarget;
        private Color currentOverlayColour;
        private bool isHovered;
        private bool enabled;
        private int buttonIndex;
        private string buttonTooltip;

        public ClickableTextureComponent Component
        {
            get => buttonComponent;
        }

        public bool Enabled
        {
            get => enabled;
            set { enabled = value; }
        }

        public bool IsHovered
        {
            get => isHovered;
            set { isHovered = value; }
        }

        public string ButtonTooltip
        {
            get => buttonTooltip;
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

        public ButtonType Type
        {
            get => buttonType;
        }

        public Action ButtonAction
        {
            get => buttonAction;
        }

        public TileFeature? LayerToTarget
        {
            get => layerToTarget;
        }

        // TODO: Possibly refactor buttons into a Button base class, and have ToolButton and LayerButton inherit from that to simplify the Draw method?
        public void Draw(SpriteBatch b)
        {
            if (Type != ButtonType.Layer)
                b.Draw(Component.texture, new Vector2(Component.bounds.X, Component.bounds.Y), Component.sourceRect, CurrentOverlayColour, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
            else
            {
                if (ModState.ActiveTool.HasValue)
                {
                    if (ModState.ActiveTool.Equals(ButtonId.Erase))
                    {
                        b.Draw(Component.texture, new Vector2(Component.bounds.X, Component.bounds.Y), Component.sourceRect, CurrentOverlayColour, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
                    }
                    else
                    {
                        b.Draw(Component.texture, new Vector2(Component.bounds.X, Component.bounds.Y), Component.sourceRect, Color.DarkSlateGray, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
                    }
                }
                else
                    b.Draw(Component.texture, new Vector2(Component.bounds.X, Component.bounds.Y), Component.sourceRect, Color.DarkSlateGray, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
            }

            if (Type == ButtonType.Tool)
            {
                if (ModState.ActiveTool.HasValue)
                {
                    if (Id.Equals(ModState.ActiveTool.Value))
                    {
                        b.Draw(
                            Game1.mouseCursors,
                            new Vector2(Component.bounds.X, Component.bounds.Y),
                            Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 29),
                            Color.Black,
                            0.0f,
                            Vector2.Zero,
                            1f,
                            SpriteEffects.None,
                            0f);
                    }
                }
            }

            if (Type == ButtonType.Layer)
            {
                if (ModState.SelectedLayer.HasValue)
                {
                    // b.Draw(
                    //     Game1.mouseCursors,
                    //     new Vector2(Component.bounds.X, Component.bounds.Y),
                    //     Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 29),
                    //     Color.White,
                    //     0.0f,
                    //     Vector2.Zero,
                    //     1f,
                    //     SpriteEffects.None,
                    //     0f);

                    if (LayerToTarget.Equals(ModState.SelectedLayer.Value))
                    {
                        b.Draw(
                            Game1.mouseCursors,
                            new Vector2(Component.bounds.X + 80, Component.bounds.Y + 8),
                            new Rectangle(352, 495, 12, 11),
                            Color.White,
                            0.0f,
                            Vector2.Zero,
                            4f,
                            SpriteEffects.None,
                            0f);

                        Utility.drawTextWithColoredShadow(b, ButtonTooltip, Game1.dialogueFont, new Vector2(Component.bounds.X + 96, Component.bounds.Y + 64), Color.WhiteSmoke, new Color(Color.Black, 0.75f));
                    }
                }


            }
        }

        /// <summary>
        /// Everything can be derived from the button ID.
        /// </summary>
        /// <param name="button"></param>
        public ToolButton(ButtonId button, ButtonType type, Action action, int index, string tooltip, Texture2D texture, TileFeature? layerToTarget = null)

        {
            Rectangle sourceRect = Ui.GetButtonSourceRect(button);
            buttonIndex = index;
            buttonTooltip = tooltip;
            buttonId = button;
            buttonType = type;
            buttonAction = action;
            this.layerToTarget = layerToTarget;
            currentOverlayColour = Color.White;

            buttonComponent = new ClickableTextureComponent(
                new Rectangle(0, 0, 0, 0),
                texture,
                sourceRect,
                1f
            );
        }
    }
}