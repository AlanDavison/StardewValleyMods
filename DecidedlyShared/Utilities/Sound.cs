using System;
using StardewValley;

namespace DecidedlyShared.Utilities;

public class Sound
{
    public static bool TryPlaySound(string soundCue)
    {
        try
        {
            Game1.soundBank.GetCue(soundCue);
        }
        catch (Exception e)
        {

            return false;
        }

        Game1.playSound(soundCue);
        return true;
    }
}
