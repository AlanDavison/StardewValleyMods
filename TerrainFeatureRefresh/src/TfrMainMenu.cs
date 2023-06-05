using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using TerrainFeatureRefresh.Framework;

namespace TerrainFeatureRefresh;

public class TfrMainMenu : IClickableMenu
{
    private Button resetButton;
    private Texture2D boxTexture;
    // private Texture2D buttonPanelTexture;
    private Rectangle titleBounds;
    private Rectangle mainWindowBounds;
    private Rectangle buttonPanelBounds;
    private TfrSettings settings;

    private List<TfrCheckbox> checkboxes;

    // Objects
    private TfrCheckbox fences;
    private TfrCheckbox weeds;
    private TfrCheckbox twigs;
    private TfrCheckbox stones;
    private TfrCheckbox forage;
    private TfrCheckbox artifactSpots;

    // Terrain Features
    private TfrCheckbox grass;
    private TfrCheckbox wildTrees;
    private TfrCheckbox fruitTrees;
    private TfrCheckbox paths;
    private TfrCheckbox hoeDirt;
    private TfrCheckbox crops;
    private TfrCheckbox bushes;

    // Resource Clumps
    private TfrCheckbox stumps;
    private TfrCheckbox logs;
    private TfrCheckbox boulders;
    private TfrCheckbox meteorites;

    private string resetButtonText;
    private string terrainFeatureHeader = "Terrain Features";
    private string objectHeader = "Objects";
    private string clumpHeader = "Resource Clumps";

    public TfrMainMenu(int screenX, int screenY, int width, int height)
        : base(screenX, screenY, width, height, true)
    {
        this.boxTexture = Game1.menuTexture;
        // this.buttonPanelTexture = Game1.content.Load<Texture2D>("Mods/DecidedlyHuman/TFR/ButtonPanel");
        this.resetButton = new Button(
            new Rectangle(
                this.xPositionOnScreen + 64,
                this.yPositionOnScreen + 64,
                0,
                0),
            "Reset Selected");
        this.checkboxes = new List<TfrCheckbox>();
        this.allClickableComponents = new List<ClickableComponent>();

        this.settings = new TfrSettings();

        // Objects
        this.checkboxes.Add(this.fences = new TfrCheckbox(Rectangle.Empty, "Fences", ref this.settings.fences));
        this.checkboxes.Add(this.weeds = new TfrCheckbox(Rectangle.Empty, "Weeds", ref this.settings.weeds));
        this.checkboxes.Add(this.twigs = new TfrCheckbox(Rectangle.Empty, "Twigs", ref this.settings.twigs));
        this.checkboxes.Add(this.stones = new TfrCheckbox(Rectangle.Empty, "Stones", ref this.settings.stones));
        this.checkboxes.Add(this.forage = new TfrCheckbox(Rectangle.Empty, "Forage", ref this.settings.forage));
        this.checkboxes.Add(this.artifactSpots = new TfrCheckbox(Rectangle.Empty, "Artifact Spot", ref this.settings.artifactSpots));

        // Terrain Features
        this.checkboxes.Add(this.grass = new TfrCheckbox(Rectangle.Empty, "Grass", ref this.settings.grass));
        this.checkboxes.Add(this.wildTrees = new TfrCheckbox(Rectangle.Empty, "Wild Trees", ref this.settings.wildTrees));
        this.checkboxes.Add(this.fruitTrees = new TfrCheckbox(Rectangle.Empty, "Fruit Trees", ref this.settings.fruitTrees));
        this.checkboxes.Add(this.paths = new TfrCheckbox(Rectangle.Empty, "Paths", ref this.settings.paths));
        this.checkboxes.Add(this.hoeDirt = new TfrCheckbox(Rectangle.Empty, "Hoed Dirt", ref this.settings.hoeDirt));
        this.checkboxes.Add(this.crops = new TfrCheckbox(Rectangle.Empty, "Crops", ref this.settings.crops));
        this.checkboxes.Add(this.bushes = new TfrCheckbox(Rectangle.Empty, "Bushes", ref this.settings.bushes));

        // Resource Clumps
        this.checkboxes.Add(this.stumps = new TfrCheckbox(Rectangle.Empty, "Stumps", ref this.settings.stumps));
        this.checkboxes.Add(this.logs = new TfrCheckbox(Rectangle.Empty, "Logs", ref this.settings.logs));
        this.checkboxes.Add(this.boulders = new TfrCheckbox(Rectangle.Empty, "Boulders", ref this.settings.boulders));
        this.checkboxes.Add(this.meteorites = new TfrCheckbox(Rectangle.Empty, "Spoiler Rocks", ref this.settings.meteorites));

        foreach (TfrCheckbox box in this.checkboxes)
        {
            this.allClickableComponents.Add(box);
        }

        this.allClickableComponents.Add(this.resetButton);

        this.UpdateBounds();
    }

    private void UpdateBounds()
    {
        this.titleBounds = new Rectangle(
            this.xPositionOnScreen,
            this.yPositionOnScreen - 64 + 32 + 16 + 8,
            this.width - 128,
            128);

        this.mainWindowBounds = new Rectangle(
            this.xPositionOnScreen,
            this.yPositionOnScreen + 32,
            this.width,
            this.height - 16);

        this.buttonPanelBounds = new Rectangle(
            this.xPositionOnScreen + this.width - 256 + 128 + 32 + 8,
            this.yPositionOnScreen + this.height - 104,
            this.width - 256 - 32,
            128
            );

        this.resetButton.bounds = new Rectangle(
            this.mainWindowBounds.Right - this.resetButton.bounds.Width - 16,
            this.mainWindowBounds.Bottom - this.resetButton.bounds.Height - 16,
            this.resetButton.bounds.Width,
            this.resetButton.bounds.Height);
        new Vector2(this.xPositionOnScreen + 16, this.yPositionOnScreen + 64 - 8);

        #region Objects

        int objectY = this.yPositionOnScreen + 64 + 24;

        this.fences.bounds = new Rectangle(
            this.xPositionOnScreen + 256 + 16,
            objectY,
            this.fences.bounds.Width,
            this.fences.bounds.Height);

        objectY += this.fences.bounds.Height + 6;

        this.weeds.bounds = new Rectangle(
            this.xPositionOnScreen + 256 + 16,
            objectY,
            this.fences.bounds.Width,
            this.fences.bounds.Height);

        objectY += this.fences.bounds.Height + 6;

        this.stones.bounds = new Rectangle(
            this.xPositionOnScreen + 256 + 16,
            objectY,
            this.fences.bounds.Width,
            this.fences.bounds.Height);

        objectY += this.fences.bounds.Height + 6;

        this.twigs.bounds = new Rectangle(
            this.xPositionOnScreen + 256 + 16,
            objectY,
            this.fences.bounds.Width,
            this.fences.bounds.Height);

        objectY += this.fences.bounds.Height + 6;

        this.forage.bounds = new Rectangle(
            this.xPositionOnScreen + 256 + 16,
            objectY,
            this.fences.bounds.Width,
            this.fences.bounds.Height);

        objectY += this.fences.bounds.Height + 6;

        this.artifactSpots.bounds = new Rectangle(
            this.xPositionOnScreen + 256 + 16,
            objectY,
            this.artifactSpots.bounds.Width,
            this.artifactSpots.bounds.Height);

        #endregion

        #region TerrainFeatures

        int terrainY = this.yPositionOnScreen + 64 + 24;

        this.grass.bounds = new Rectangle(
            this.xPositionOnScreen + 16,
            terrainY,
            this.grass.bounds.Width,
            this.grass.bounds.Height);

        terrainY += this.grass.bounds.Height + 6;

        this.wildTrees.bounds = new Rectangle(
            this.xPositionOnScreen + 16,
            terrainY,
            this.grass.bounds.Width,
            this.grass.bounds.Height);

        terrainY += this.grass.bounds.Height + 6;

        this.fruitTrees.bounds = new Rectangle(
            this.xPositionOnScreen + 16,
            terrainY,
            this.grass.bounds.Width,
            this.grass.bounds.Height);

        terrainY += this.grass.bounds.Height + 6;

        this.hoeDirt.bounds = new Rectangle(
            this.xPositionOnScreen + 16,
            terrainY,
            this.grass.bounds.Width,
            this.grass.bounds.Height);

        terrainY += this.grass.bounds.Height + 6;

        this.crops.bounds = new Rectangle(
            this.xPositionOnScreen + 16,
            terrainY,
            this.grass.bounds.Width,
            this.grass.bounds.Height);

        terrainY += this.grass.bounds.Height + 6;

        this.bushes.bounds = new Rectangle(
            this.xPositionOnScreen + 16,
            terrainY,
            this.grass.bounds.Width,
            this.grass.bounds.Height);

        terrainY += this.grass.bounds.Height + 6;

        this.paths.bounds = new Rectangle(
            this.xPositionOnScreen + 16,
            terrainY,
            this.grass.bounds.Width,
            this.grass.bounds.Height);

        #endregion

        #region ResourceClumps

        int clumpY = this.yPositionOnScreen + 64 + 24;

        this.stumps.bounds = new Rectangle(
            this.xPositionOnScreen + 256 + 128 + 32,
            clumpY,
            this.stumps.bounds.Width,
            this.stumps.bounds.Height);

        clumpY += this.stumps.bounds.Height + 6;

        this.logs.bounds = new Rectangle(
            this.xPositionOnScreen + 256 + 128 + 32,
            clumpY,
            this.stumps.bounds.Width,
            this.stumps.bounds.Height);

        clumpY += this.stumps.bounds.Height + 6;

        this.boulders.bounds = new Rectangle(
            this.xPositionOnScreen + 256 + 128 + 32,
            clumpY,
            this.stumps.bounds.Width,
            this.stumps.bounds.Height);

        clumpY += this.stumps.bounds.Height + 6;

        this.meteorites.bounds = new Rectangle(
            this.xPositionOnScreen + 256 + 128 + 32,
            clumpY,
            this.stumps.bounds.Width,
            this.stumps.bounds.Height);

        #endregion
    }

    public override void draw(SpriteBatch b)
    {
        this.UpdateBounds();

        DecidedlyShared.Ui.Utils.DrawBox(
            b,
            this.boxTexture,
            new Rectangle(0, 256, 60, 60),
            this.titleBounds);

        // DecidedlyShared.Ui.Utils.DrawBox(
        //     b,
        //     this.buttonPanelTexture,
        //     new Rectangle(0, 0, 84, 108),
        //     this.buttonPanelBounds,
        //     Color.White,
        //     40,
        //     12,
        //     40,
        //     40);

        DecidedlyShared.Ui.Utils.DrawBox(
            b,
            this.boxTexture,
            new Rectangle(0, 256, 60, 60),
            this.mainWindowBounds);

        base.draw(b);

        Vector2 stringWidth = Game1.smallFont.MeasureString("Terrain Feature Refresh");

        Utility.drawTextWithShadow(
            b,
            "Terrain Feature Refresh",
            Game1.smallFont,
            new Vector2(this.xPositionOnScreen + stringWidth.X / 2 - 32, this.yPositionOnScreen + 4),
            Game1.textColor);

        Utility.drawTextWithShadow(
            b,
            this.terrainFeatureHeader,
            Game1.smallFont,
            new Vector2(this.xPositionOnScreen + 16, this.yPositionOnScreen + 64 - 8),
            Game1.textColor);

        Utility.drawTextWithShadow(
            b,
            this.objectHeader,
            Game1.smallFont,
            new Vector2(this.xPositionOnScreen + 256 + 16, this.yPositionOnScreen + 64 - 8),
            Game1.textColor);

        Utility.drawTextWithShadow(
            b,
            this.clumpHeader,
            Game1.smallFont,
            new Vector2(this.xPositionOnScreen + 256 + 128 + 32, this.yPositionOnScreen + 64 - 8),
            Game1.textColor);

        this.resetButton.Draw(b);

        // Objects
        this.fences.Draw(b);
        this.weeds.Draw(b);
        this.twigs.Draw(b);
        this.stones.Draw(b);
        this.forage.Draw(b);
        this.artifactSpots.Draw(b);

        // Terrain Features
        this.wildTrees.Draw(b);
        this.fruitTrees.Draw(b);
        this.paths.Draw(b);
        this.hoeDirt.Draw(b);
        this.crops.Draw(b);
        this.bushes.Draw(b);
        this.grass.Draw(b);

        // Resource Clumps
        this.stumps.Draw(b);
        this.logs.Draw(b);
        this.boulders.Draw(b);
        this.meteorites.Draw(b);

        base.drawMouse(b);
    }

    public override void receiveLeftClick(int x, int y, bool playSound = true)
    {
        foreach (TfrCheckbox box in this.checkboxes)
        {
            if (box.containsPoint(x, y))
                box.ReceiveLeftClick();
        }

        // TODO: Rig up close button click!
    }

    public override void performHoverAction(int x, int y)
    {
        this.resetButton.DoHover(x, y);

        // This call is required for the close button hover to work.
        base.performHoverAction(x, y);
    }

    public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
    {
        base.gameWindowSizeChanged(oldBounds, newBounds);
        this.UpdateBounds();
        this.upperRightCloseButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + this.width - 36, this.yPositionOnScreen - 8, 48, 48), Game1.mouseCursors, new Rectangle(337, 494, 12, 12), 4f);
    }
}
