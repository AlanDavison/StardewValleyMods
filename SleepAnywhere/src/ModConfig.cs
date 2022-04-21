using System.Collections.Generic;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley.SDKs;

namespace SleepAnywhere
{
    public class ModConfig
    {
        public KeybindList PlaceSleepingBag = KeybindList.Parse("OemTilde");
        public KeybindList GrabSleepingBagRemotely = KeybindList.Parse("OemPeriod");
        
        // The cheese zone.
        public bool RequireSleepingBagItem = true;
    }
}