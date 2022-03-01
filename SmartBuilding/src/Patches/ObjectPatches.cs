using StardewValley;

namespace SmartBuilding.Patches
{
	public static class ObjectPatches
	{
		private static bool _currentlyDrawing;

		public static bool CurrentlyDrawing
		{
			get { return _currentlyDrawing; }
			set { _currentlyDrawing = value; }
		}

		public static bool PlacementAction_Prefix(Object __instance, GameLocation location, int x, int y, Farmer who)
		{ // If we're currently drawing, we absolutely do not want objects to be placeable.
			return !_currentlyDrawing;
		}
	}
}