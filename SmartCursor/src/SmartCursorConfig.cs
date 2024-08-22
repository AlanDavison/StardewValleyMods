using System.Collections.Generic;
using StardewModdingAPI;

namespace SmartCursor
{
    public class SmartCursorConfig
    {
        // Keybinds
        public SButton SmartCursorHold = SButton.LeftControl;

        // Tool hit ranges
        public int TierOneRange = 1;
        public int TierTwoRange = 2;
        public int TierThreeRange = 3;
        public int TierFourRange = 4;
        public int TierFiveRange = 5;
        public int TierSixRange = 6;
        public int TierSevenRange = 7;

        // Toggles
        public bool AllowTargetingBabyTrees = false;
        public bool AllowTargetingGiantCrops = false;
        public bool AllowTargetingTappedTrees = false;

        // This is bad. But... it'll do for now.
        public void GetToolRanges(out Dictionary<int, int> ranges)
        {
            ranges = new Dictionary<int, int>();

            ranges[0] = this.TierOneRange;
            ranges[1] = this.TierTwoRange;
            ranges[2] = this.TierThreeRange;
            ranges[3] = this.TierFourRange;
            ranges[4] = this.TierFiveRange;
        }
    }
}
