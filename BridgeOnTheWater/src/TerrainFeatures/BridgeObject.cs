using StardewValley;

namespace PortableBridges.TerrainFeatures
{
    public class BridgeObject : Fence
    {
        // public readonly NetInt whichFloor = new NetInt();
        // public readonly NetInt whichView = new NetInt();
        // public readonly NetBool isPathway = new NetBool();
        // public readonly NetBool isSteppingStone = new NetBool();
        // public readonly NetBool drawContouredShadow = new NetBool();
        // public readonly NetBool cornerDecoratedBorders = new NetBool();

        public BridgeObject()
        {
            this.whichType.Value = 2;
        }

        public override void actionOnPlayerEntry()
        {
            Game1.player.ignoreCollisions = true;
        }
    }
}
