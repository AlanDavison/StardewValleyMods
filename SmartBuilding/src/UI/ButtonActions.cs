namespace SmartBuilding.UI
{
    public class ButtonActions
    {
        private ModEntry mod;
        
        // Simply passing in our main mod class is terrible, but... it's either this, or spend
        // forever rearchitecting when I could be working on the usability of the mod.
        public ButtonActions(ModEntry mod)
        {
            this.mod = mod;
        }
        
        public void DrawClicked()
        {
            ModState.ActiveTool = ButtonId.Draw;
        }

        public void EraseClicked()
        {
            ModState.ActiveTool = ButtonId.Erase;
        }

        public void FilledRectangleClicked()
        {
            ModState.ActiveTool = ButtonId.FilledRectangle;
        }

        public void DrawnLayerClicked()
        {
            ModState.SelectedLayer = TileFeature.Drawn;
        }

        public void ObjectLayerClicked()
        {
            ModState.SelectedLayer = TileFeature.Object;
        }

        public void TerrainFeatureLayerClicked()
        {
            ModState.SelectedLayer = TileFeature.TerrainFeature;
        }

        public void FurnitureLayerClicked()
        {
            ModState.SelectedLayer = TileFeature.Furniture;
        }

        public void InsertClicked()
        {
            ModState.ActiveTool = ButtonId.Insert;
        }

        public void ConfirmBuildClicked()
        {
            mod.ConfirmBuild();
        }

        public void ClearBuildClicked()
        {
            mod.ClearBuild();
        }
    }
}