using StardewModdingAPI.Utilities;

namespace BetterReturnScepter
{
    public class ModConfig
    {
        public bool CountWarpMenuAsScepterUsage = false;
        public bool EnableMultiObeliskSupport = false;
        public KeybindList OpenObeliskWarpMenuController = KeybindList.Parse("BigButton");
        public KeybindList OpenObeliskWarpMenuKbm = KeybindList.Parse("OemTilde");
        public KeybindList ReturnToLastPoint = KeybindList.Parse("LeftStick");
    }
}
