﻿using Microsoft.Xna.Framework;
using StardewValley;
// ReSharper disable InconsistentNaming
// ReSharper disable InvalidXmlDocComment

namespace DecidedlyShared.APIs
{
    /// <summary>
    ///     The API for this mod.
    /// </summary>
    public interface ITapGiantCropsAPI
    {
        /// <summary>
        ///     Checks whether or not a tapper can be added to this particular tile.
        /// </summary>
        /// <param name="loc">Game location.</param>
        /// <param name="tile">Tile.</param>
        /// <returns>True if placeable, false otherwise.</returns>
        public bool CanPlaceTapper(GameLocation loc, Vector2 tile, SObject obj);

        /// <summary>
        ///     Called to place a tapper a the specific tile.
        /// </summary>
        /// <param name="loc">GameLocation.</param>
        /// <param name="tile">Tile.</param>
        /// <returns>True if successfully placed, false otherwise.</returns>
        public bool TryPlaceTapper(GameLocation loc, Vector2 tile, SObject obj);
    }
}
