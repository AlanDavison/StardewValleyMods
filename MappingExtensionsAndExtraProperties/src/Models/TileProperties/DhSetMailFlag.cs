namespace MappingExtensionsAndExtraProperties.Models.TileProperties;

public struct DhSetMailFlag
{
    public static string TileProperty = "DHSetMailFlag";
    private string mailFlag;

    public string MailFlag
    {
        get => this.mailFlag;
    }

    public DhSetMailFlag(string mailFlag)
    {
        this.mailFlag = mailFlag;
    }
}
