﻿using Microsoft.Xna.Framework;
using StardewValley;
// ReSharper disable InconsistentNaming
// ReSharper disable UseSymbolAlias
// ReSharper disable RedundantNameQualifier

namespace DecidedlyShared.APIs
{
    /// <summary>
    /// The API for More Fertilizers, which handles placement of fertilizers by
    /// routes the game does not usually support.
    /// </summary>
    public interface IMoreFertilizersAPI
    {
        /// <summary>
        /// Checks whether or not a fertilizer can be placed at a specific tile.
        /// </summary>
        /// <param name="obj">StardewValley.Object to place.</param>
        /// <param name="loc">GameLocation to place at.</param>
        /// <param name="tile">Tile to place at.</param>
        /// <returns>True if the fertilizer can be placed, false otherwise.</returns>
        public bool CanPlaceFertilizer(StardewValley.Object obj, GameLocation loc, Vector2 tile);

        /// <summary>
        /// Called to place a fertilizer at a specific location/tile.
        /// </summary>
        /// <param name="obj">StardewValley.Object to place.</param>
        /// <param name="loc">GameLocation to place at.</param>
        /// <param name="tile">Which tile to place at.</param>
        /// <returns>True if successfully placed, false otherwise.</returns>
        /// <remarks>Does not handle inventory management or animations.</remarks>
        public bool TryPlaceFertilizer(StardewValley.Object obj, GameLocation loc, Vector2 tile);

        /// <summary>
        /// Animates the placement of a fertilizer at a specific location.
        /// </summary>
        /// <param name="obj">StardewValley.Object to place.</param>
        /// <param name="loc">GameLocation to place at.</param>
        /// <param name="tile">Which tile to place at.</param>
        public void AnimateFertilizer(StardewValley.Object obj, GameLocation loc, Vector2 tile);
    }
}
