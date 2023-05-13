using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace EveryoneIsGrandpa.src
{
    public static class Patches
    {
        private static AnimatedSprite grandpa;
        private static List<NPC> npcsWithChangedSprites;

        public static void Init()
        {
            grandpa = new AnimatedSprite("Characters/DecidedlyHumansCurse/Grandpa", 0, 16, 32);
            npcsWithChangedSprites = new List<NPC>();
        }

        public static bool NPC_draw_prefix(NPC __instance, SpriteBatch b, float alpha = 1f)
        {
            if (!npcsWithChangedSprites.Contains(__instance))
            {
                __instance.Sprite = grandpa;
                npcsWithChangedSprites.Add(__instance);
            }

            return true;
        }
    }
}
