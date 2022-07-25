using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace SmartBuilding.Constants
{
    public class Directions
    {
        public static List<Vector2> vector2 = new List<Vector2>()
        {
            new Vector2(0, 0), // Centre
            new Vector2(-1, 0), // Left
            new Vector2(1, 0), // Right
            new Vector2(0, -1), // Up
            new Vector2(0, 1), // Down
            new Vector2(-1, -1), // Up left
            new Vector2(1, -1), // Up right
            new Vector2(-1, 1), // Down left
            new Vector2(1, 1) // Down right
        };
    }
}