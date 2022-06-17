using SmartBuilding.UI;

namespace SmartBuilding
{
    public static class ModState
    {
        private static bool inBuildMode = false;
        private static bool blockMouseInteractions = false;
        private static ButtonId? activeTool;
        private static TileFeature? selectedLayer;
        
        public static TileFeature? SelectedLayer
        {
            get => selectedLayer;
            set
            {
                selectedLayer = value;
            }
        }
        
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

        /// <summary>
        /// Whether or not mouse buttons should apply to our UI only.
        /// </summary>
        public static bool BlockMouseInteractions
        {
            get => blockMouseInteractions;
            set { blockMouseInteractions = value; }
        }
    }
}