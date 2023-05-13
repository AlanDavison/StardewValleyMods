using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using StardewValley;
using StardewValley.Minigames;

namespace ATropicalStart;

public static class Patches
{
    public static bool Intro_tick_prefix(Intro __instance, GameTime time)
    {
        Game1.warpFarmer("Island_W", 58, 88, flip: false);
        if (Intro.roadNoise != null)
        {
            Intro.roadNoise.Stop(AudioStopOptions.Immediate);
        }
        __instance.doneCreatingCharacter();
        Game1.exitActiveMenu();
        Game1.currentMinigame = null;
        __instance.forceQuit();
        __instance.unload();

        return false;
    }

}
