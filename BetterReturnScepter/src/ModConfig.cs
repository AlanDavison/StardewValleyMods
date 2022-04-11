using StardewModdingAPI.Utilities;

namespace BetterReturnScepter
{
    public class ModConfig
    {
        public bool EnableMultiObeliskSupport = false;
        public bool CountWarpMenuAsScepterUsage = false;
        public KeybindList ReturnToLastPoint = KeybindList.Parse("LeftStick");
        public KeybindList OpenObeliskWarpMenuKbm = KeybindList.Parse("OemTilde");
        public KeybindList OpenObeliskWarpMenuController = KeybindList.Parse("BigButton");
    }
}