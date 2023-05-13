using StardewValley;

namespace DecidedlyShared.Utilities;

public class Player
{
    public static bool TryAddMailFlag(string flag, Farmer player)
    {
        if (!player.hasOrWillReceiveMail(flag))
        {
            player.mailReceived.Add(flag);

            return true;
        }

        return false;
    }
}
