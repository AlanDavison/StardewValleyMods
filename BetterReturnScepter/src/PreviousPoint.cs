using Microsoft.Xna.Framework;
using StardewValley;

namespace BetterReturnScepter
{
    public class PreviousPoint
    {
        /// <summary>
        ///     The GameLocation the player used the return sceptre's vanilla function in.
        /// </summary>
        public GameLocation Location { get; set; }

        /// <summary>
        ///     The tile (as a Vector2) the player was on when using the return sceptre.
        /// </summary>
        public Vector2 Tile { get; set; } = new();
    }
}
