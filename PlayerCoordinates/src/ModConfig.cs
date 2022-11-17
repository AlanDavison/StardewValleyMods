using StardewModdingAPI;

namespace PlayerCoordinates
{
    public class ModConfig
    {
        public SButton CoordinateHUDToggle { get; set; } = SButton.F5;
        public SButton LogCoordinates { get; set; } = SButton.F6;
        public SButton SwitchToCursorCoords { get; set; } = SButton.F7;
        public SButton HudUnlock { get; set; } = SButton.F8;
        public bool LogTrackingTarget { get; set; } = true;
        public int HudXCoord { get; set; }
        public int HudYCoord { get; set; }
    }
}
