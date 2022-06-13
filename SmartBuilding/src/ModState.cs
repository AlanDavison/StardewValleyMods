using SmartBuilding.UI;

namespace SmartBuilding
{
    public static class ModState
    {
        private static bool inBuildMode = false;
        private static ButtonId? activeTool;
        
        public static void EnterBuildMode()
        {
            
        }

        public static void LeaveBuildMode()
        {
            ActiveTool = null;
        }

        public static ButtonId? ActiveTool
        {
            get { return activeTool; }
            set { activeTool = value; }
        }

        public static bool InBuildMode
        {
            get => inBuildMode;
        }
    }
}