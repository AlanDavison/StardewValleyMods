namespace MappingExtensionsAndExtraProperties.Models.TileProperties;

public struct SetMailFlag : ITilePropertyData
{
    public static string PropertyKey => "MEEP_SetMailFlag";
    private string mailFlag;

    public string MailFlag
    {
        get => this.mailFlag;
    }

    public SetMailFlag(string mailFlag)
    {
        this.mailFlag = mailFlag;
    }
}
