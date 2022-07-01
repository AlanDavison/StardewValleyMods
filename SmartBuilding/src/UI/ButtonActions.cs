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

        private void ResetTileState()
        {
            mod.ResetVolatileTiles();
        }
        
        public void DrawClicked()
        {
            ModState.ActiveTool = ButtonId.Draw;
            ResetTileState();
        }

        public void EraseClicked()
        {
            ModState.ActiveTool = ButtonId.Erase;
            ResetTileState();
        }

        public void FilledRectangleClicked()
        {
            ModState.ActiveTool = ButtonId.FilledRectangle;
            ResetTileState();
        }

        public void DrawnLayerClicked()
        {
            ModState.SelectedLayer = TileFeature.Drawn;
            ResetTileState();
        }

        public void ObjectLayerClicked()
        {
            ModState.SelectedLayer = TileFeature.Object;
            ResetTileState();
        }

        public void TerrainFeatureLayerClicked()
        {
            ModState.SelectedLayer = TileFeature.TerrainFeature;
            ResetTileState();
        }

        public void FurnitureLayerClicked()
        {
            ModState.SelectedLayer = TileFeature.Furniture;
            ResetTileState();
        }

        public void InsertClicked()
        {
            ModState.ActiveTool = ButtonId.Insert;
            ResetTileState();
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