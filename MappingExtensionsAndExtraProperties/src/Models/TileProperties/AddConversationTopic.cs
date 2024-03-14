namespace MappingExtensionsAndExtraProperties.Models.TileProperties;

public class AddConversationTopic : ITilePropertyData
{
    public static string PropertyKey => "MEEP_AddConversationTopic";
    private string conversationTopic;
    private int numberOfDays;

    public string ConversationTopic
    {
        get => this.conversationTopic;
    }

    public int Days
    {
        get => this.numberOfDays;
    }

    public AddConversationTopic(string ct, int days)
    {
        this.conversationTopic = ct;
        this.numberOfDays = days;
    }
}
