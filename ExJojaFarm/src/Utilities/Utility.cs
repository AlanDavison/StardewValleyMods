using Microsoft.Xna.Framework;

namespace ExJojaFarm.Utilities;

public class Utility
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="val">Space-delimited float string.</param>
    /// <param name="parsedVector">The parsed Vector2.</param>
    /// <returns></returns>
    public static bool TryParseVector2(string val, out Vector2 parsedVector)
    {
        parsedVector = Vector2.Zero;

        string[] stringValues = val.Split(" ");

        if (stringValues.Length != 2)
        {
            ModEntry.StaticLogger.Error($"Couldn't parse input string \"{val}\" as two integers.");
            return false;
        }

        if (!int.TryParse(stringValues[0], out int x) || !int.TryParse(stringValues[1], out int y))
        {
            ModEntry.StaticLogger.Error($"Couldn't parse one or both split strings \"{stringValues[0]}\" or \"{stringValues[1]}\" as an integer.");
            return false;
        }

        parsedVector = new Vector2(x, y);

        return true;
    }
}
