using StardewUI;
using StardewUI.Widgets;

namespace MappingExtensionsAndExtraProperties.Models.WarpStations.UI;

public class WarpSystemTargetView : ComponentView<Frame>
{
    private WarpStationTarget targetData;

    public WarpSystemTargetView(WarpStationTarget data)
    {
        this.targetData = data;
    }

    protected override Frame CreateView()
    {
        return null;
    }
}
