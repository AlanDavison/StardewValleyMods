using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using Rectangle = xTile.Dimensions.Rectangle;

namespace MappingExtensionsAndExtraProperties.Models.CustomBackgrounds;

public class CustomBackground : Background
{
    private readonly BackgroundData backgroundData;
    private readonly Dictionary<string, Texture2D> backgroundTextures;
    private readonly List<BackgroundElement> backgroundElements;
    private readonly BackgroundScene scene;

    private enum Anchor
    {
        Left,
        Right
    }

    public CustomBackground(BackgroundData? backgroundData) : base(Game1.currentLocation, Color.White, true)
    {
        this.backgroundData = backgroundData;
        this.backgroundTextures = new Dictionary<string, Texture2D>();
        this.scene = backgroundData.Scene!;
        this.backgroundElements = this.scene.Elements!;

        foreach (BackgroundImage image in this.backgroundData.Images!)
        {
            if (image.ImageId is null)
                continue;

            Texture2D texture = Game1.content.Load<Texture2D>(image.Texture);

            if (texture is null)
                continue;

            this.backgroundTextures.Add(image.ImageId!, texture);
        }

        foreach (BackgroundElement element in this.backgroundElements)
        {
            element.x = element.BaseX;
            element.y = element.BaseY;
        }
    }

    public override void update(Rectangle viewport)
    {
        int mapHorizontalCentre = Game1.currentLocation.Map.DisplayWidth / 2 / Game1.tileSize;
        int mapVerticalCentre = Game1.currentLocation.Map.DisplayHeight / 2 / Game1.tileSize;
        int viewportHCentre = viewport.X + viewport.Width / 2;
        int viewportVCentre = viewport.Y + viewport.Height / 2;
        float xOffset = viewportHCentre - mapHorizontalCentre;
        float yOffset = viewportVCentre - mapVerticalCentre;

        // I don't think there is a way to make this work mathematically without having anchor sides for our elements.
        // No big deal, though. TODO: Add anchor sides to background elements.

        if (Math.Sign(xOffset) < 0)
        {

        }

        foreach (var element in this.scene.Elements)
        {
            element.x = -viewport.X;
            element.x += -xOffset * element.HorizontalParallaxFactor * 0.1f;
            // element.Position.x = element.Position.x * element.Position.HorizontalParallaxFactor;
            // element.Position.x = (viewport.X) * element.Position.HorizontalParallaxFactor * 0.25f;
            element.y = -viewport.Y;


        }
    }

    public override void draw(SpriteBatch b)
    {
        foreach (var element in this.scene.Elements.OrderBy(element => element.Depth).Reverse())
        {
            Vector2 position = new Vector2(element.x, element.y);

            Texture2D texture = this.backgroundTextures[element.ImageId];
            b.Draw(texture,
                position,
                texture.Bounds,
                Color.White,
                0f,
                Vector2.Zero,
                new Vector2(Game1.pixelZoom * Game1.options.desiredBaseZoomLevel, Game1.pixelZoom * Game1.options.desiredBaseZoomLevel),
                SpriteEffects.None,
                0f);
        }
    }
}
