using Microsoft.Xna.Framework;
using SpaceCore.UI;
using StardewModdingAPI;
using StardewValley;

namespace UITestbed;

public class ModEntry : Mod
{
    public override void Entry(IModHelper helper)
    {
        RootElement root = new RootElement();
        Label label = new Label();
        label.Font = Game1.dialogueFont;
        label.String = "AAAAAAAAA";
        label.LocalPosition = new Vector2(0, 0);
        root.AddChild(label);

        helper.Events.Display.RenderedHud += (sender, args) =>
        {
            // root.Update();
            // root.Draw(args.SpriteBatch);
        };

        helper.Events.Input.ButtonPressed += (sender, args) =>
        {

        };
    }
}
