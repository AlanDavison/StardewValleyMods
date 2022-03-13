﻿using System.Collections.Generic;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley.SDKs;

namespace SmartBuilding
{
	public class ModConfig
	{
		// TODO: Add an "instantly build" toggle.
		public KeybindList EngageBuildMode = KeybindList.Parse("LeftShift+B");
		public KeybindList HoldToDraw = KeybindList.Parse("MouseRight");
		public KeybindList HoldToErase = KeybindList.Parse("LeftShift");
		public KeybindList ConfirmBuild = KeybindList.Parse("MouseLeft");
		public bool ShowBuildQueue = true;
		public bool CrabPotsInAnyWaterTile = false;
		public bool EnableReplacingFences = false;
		public bool EnableReplacingFloors = false;
		public bool LessRestrictiveFloorPlacement = false;
		public bool EnablePlantingCrops = false;
		public bool EnableCropFertilizers = false;
		public bool EnableTreeFertilizers = false;
		public bool EnableTreeTappers = false;
	}
}