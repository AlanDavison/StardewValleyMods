using System;
using FenceHopper.Model;
using Microsoft.Xna.Framework;
using StardewValley;

namespace FenceHopper.Utility;

public class HopUtilities
{
    public static bool TryGetTileToHopTo(Vector2 interactionTile, Vector2 playerTile, GameLocation location,
        out Vector2 destinationTile)
    {
        destinationTile = playerTile;

        if (!location.Objects.TryGetValue(interactionTile, out SObject interactedObject))
            return false;
        if (interactedObject is not Fence)
            return false;
        if (!TryGetDirectionOfInteractedObject(interactionTile, playerTile, out Direction interactionDirection))
            return false;

        switch (interactionDirection)
        {
            case Direction.UP:
                destinationTile = interactionTile + new Vector2(0, -1);
                return DecidedlyShared.Utilities.Locations.IsTileEmpty(location, destinationTile);
            case Direction.DOWN:
                destinationTile = interactionTile + new Vector2(0, 1);
                return DecidedlyShared.Utilities.Locations.IsTileEmpty(location, destinationTile);
            case Direction.LEFT:
                destinationTile = interactionTile + new Vector2(-1, 0);
                return DecidedlyShared.Utilities.Locations.IsTileEmpty(location, destinationTile);
            case Direction.RIGHT:
                destinationTile = interactionTile + new Vector2(1, 0);
                return DecidedlyShared.Utilities.Locations.IsTileEmpty(location, destinationTile);
            case Direction.UP_LEFT:
                destinationTile = interactionTile + new Vector2(-1, -1);
                return DecidedlyShared.Utilities.Locations.IsTileEmpty(location, destinationTile);
            case Direction.UP_RIGHT:
                destinationTile = interactionTile + new Vector2(1, -1);
                return DecidedlyShared.Utilities.Locations.IsTileEmpty(location, destinationTile);
            case Direction.DOWN_LEFT:
                destinationTile = interactionTile + new Vector2(-1, 1);
                return DecidedlyShared.Utilities.Locations.IsTileEmpty(location, destinationTile);
            case Direction.DOWN_RIGHT:
                destinationTile = interactionTile + new Vector2(1, 1);
                return DecidedlyShared.Utilities.Locations.IsTileEmpty(location, destinationTile);
            default:
                return false;
        }
    }

    // TODO: Refactor this below method to simple take in the booleans, and return the destination tile.

    public static bool TryGetDirectionOfInteractedObject(Vector2 interactionTile, Vector2 playerTile,
        out Direction direction)
    {
        direction = Direction.UP;
        Vector2 tileDelta = interactionTile - playerTile;
        bool isLeftOf = (interactionTile.X - playerTile.X) < 0 ? true : false;
        bool isRightOf = (interactionTile.X - playerTile.X) > 0 ? true : false;
        bool isAboveOf = (interactionTile.Y - playerTile.Y) < 0 ? true : false;
        bool isBelowOf = (interactionTile.Y - playerTile.Y) > 0 ? true : false;

        switch (isLeftOf, isRightOf, isBelowOf, isAboveOf)
        {
            case (false, false, false, true):
                direction = Direction.UP;
                return true;
            case (false, false, true, false):
                direction = Direction.DOWN;
                return true;
            case (true, false, false, false):
                direction = Direction.LEFT;
                return true;
            case (false, true, false, false):
                direction = Direction.RIGHT;
                return true;
            case (true, false, false, true):
                direction = Direction.UP_LEFT;
                return true;
            case (false, true, false, true):
                direction = Direction.UP_RIGHT;
                return true;
            case (true, false, true, false):
                direction = Direction.DOWN_LEFT;
                return true;
            case (false, true, true, false):
                direction = Direction.DOWN_RIGHT;
                return true;
            default:
                return false;
        }
    }
}
