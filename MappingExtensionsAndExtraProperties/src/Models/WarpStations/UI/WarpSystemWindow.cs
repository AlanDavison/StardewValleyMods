using Microsoft.Xna.Framework.Graphics;
using StardewUI;
using StardewUI.Graphics;
using StardewUI.Layout;
using StardewUI.Widgets;
using StardewValley;

namespace MappingExtensionsAndExtraProperties.Models.WarpStations.UI;

public class WarpSystemWindow : ViewMenu<Lane>
{
    private WarpStationData stationData;

    public WarpSystemWindow(WarpStationData data)
    {
        this.stationData = data;
    }

    public WarpSystemWindow()
    {

    }

    protected override Lane CreateView()
    {
        Lane window = new Lane()
        {
            Layout = new LayoutParameters()
            {
                Width = Length.Content(),
                Height = Length.Px(800)
            },
            Orientation = Orientation.Horizontal,
            HorizontalContentAlignment = Alignment.Start
        };

        ScrollableFrameView detailsFrame = new ScrollableFrameView()
        {
            Layout = new LayoutParameters()
            {
                Width = Length.Content(),
                Height = Length.Stretch()
            }
        };

        Lane detailsPanel = new Lane()
        {
            Layout = new LayoutParameters()
            {
                Width = Length.Content(),
                Height = Length.Stretch()
            },
            Orientation = Orientation.Vertical,
            VerticalContentAlignment = Alignment.Start
        };
        detailsPanel.Children.Add(
            new Image()
            {
                Sprite = new Sprite(Game1.content.Load<Texture2D>("LooseSprites/Farm_ranching_map")),
                Scale = 4f,
                Layout = LayoutParameters.FitContent()
            });
        detailsPanel.Children.Add(
            new Label()
            {
                Text = "Test Entry"
            });

        detailsPanel.Children.Add(
            new Label()
            {
                Text = "This is a farm or something probably. It's probably pretty, I guess."
            });

        detailsFrame.Content = detailsPanel;

        ScrollableFrameView locationsPanel = new ScrollableFrameView()
        {
            Layout = new LayoutParameters()
            {
                Width = Length.Px(300),
                Height = Length.Stretch()
            }
        };

        Lane locations = new Lane()
        {
            Orientation = Orientation.Vertical
        };

        for (int i = 0; i < 100; i++)
        {
            locations.Children.Add(new Label() {Text = $"Location {i}"});
        }

        locationsPanel.Content = locations;
        window.Children.Add(detailsFrame);
        window.Children.Add(locationsPanel);

        return window;
    }
}
