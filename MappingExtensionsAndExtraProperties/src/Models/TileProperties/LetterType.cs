using System;
using DecidedlyShared.Logging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;

namespace MappingExtensionsAndExtraProperties.Models.TileProperties;

public struct LetterType : ITilePropertyData
{
    public static string PropertyKey => "MEEP_Letter_Type";
    // private bool hasCustomTexture;
    // private Texture2D texture;
    // private Rectangle sourceRect;
    public int BgType;

    // public bool HasCustomTexture => this.hasCustomTexture;
    // public Texture2D Texture => this.texture;
    // public Rectangle SourceRect => this.sourceRect;

    public LetterType(int type)
    {
        // Texture2D tex = helper.GameContent.Load<Texture2D>("LooseSprites\\letterBG");
        // Rectangle rect;

        // switch (type)
        // {
        //     case 0:
        //         rect = new Rectangle(0, 0, 320, 180);
        //         break;
        //     case 1:
        //         rect = new Rectangle(320, 0, 320, 180);
        //         break;
        //     case 2:
        //         rect = new Rectangle(640, 0, 320, 180);
        //         break;
        //     case 3:
        //         rect = new Rectangle(960, 0, 320, 180);
        //         break;
        //     default:
        //         rect = new Rectangle(0, 0, 320, 180);
        //         break;
        // }

        if (type < 0 || type > 3)
            this.BgType = 1;
        else
            this.BgType = type;
        // this.texture = tex;
        // this.sourceRect = rect;
        // this.hasCustomTexture = true;
    }

    // public LetterType(Texture2D texture, Rectangle sourceRect)
    // {
    //     this.texture = texture;
    //     this.sourceRect = sourceRect;
    //     this.hasCustomTexture = false;
    // }
}
