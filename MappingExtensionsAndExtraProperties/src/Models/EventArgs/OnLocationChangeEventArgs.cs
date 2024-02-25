using StardewValley;

namespace MappingExtensionsAndExtraProperties.Models.EventArgs;

public class OnLocationChangeEventArgs : System.EventArgs
{
    public GameLocation OldLocation;
    public GameLocation NewLocation;
    public Farmer Player;
}
