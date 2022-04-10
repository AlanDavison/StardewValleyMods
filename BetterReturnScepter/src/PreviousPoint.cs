using System;
using Microsoft.Xna.Framework;
using StardewValley;

namespace BetterReturnScepter
{
    public class PreviousPoint
    {
        private GameLocation location;
        private Vector2 tile = new Vector2();
     
        /// <summary>
        /// The GameLocation the player used the return sceptre's vanilla function in.
        /// </summary>
        public GameLocation Location
        {
            get { return location; }
            set { location = value; }
        }
        
        /// <summary>
        /// The tile (as a Vector2) the player was on when using the return sceptre.
        /// </summary>
        public Vector2 Tile
        {
            get { return tile; }
            set { tile = value; }
        }
    }
}