using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;

namespace FenceHopper;

public class ModEntry : Mod
{
    public override void Entry(IModHelper helper)
    {
        helper.Events.Input.ButtonPressed += (sender, args) =>
        {
            if (!Context.IsWorldReady)
                return;

            InputButton[] buttons = Game1.options.actionButton;
            bool actionButtonPressed = false;

            foreach (InputButton button in buttons)
            {
                if (button.ToSButton() == args.Button)
                    actionButtonPressed = true;
            }

            if (!actionButtonPressed)
                return;

            if (Utility.HopUtilities.TryGetTileToHopTo(Game1.currentCursorTile, Game1.player.Tile,
                    Game1.player.currentLocation, out Vector2 destinationTile))
            {
                Game1.player.Position = destinationTile * 64;
            }
        };
    }
}
