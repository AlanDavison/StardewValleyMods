using StardewValley;

namespace MappingExtensionsAndExtraProperties.Functionality;

public class ConversationTopic
{
    public static bool SetConversationTopic(string topic, int days)
    {
        if (!Game1.player.activeDialogueEvents.ContainsKey(topic))
            Game1.player.activeDialogueEvents.TryAdd(topic, days);

        return false;
    }
}
