using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace MappingExtensionsAndExtraProperties.Functionality;

public class AnimalDialogueBox : DialogueBox
{
    private Texture2D portrait;
    private Rectangle portraitSourceRect;
    private NPC npc;

    public AnimalDialogueBox(Texture2D animalPortrait, Rectangle portraitSourceRect, Dialogue dialogue, NPC npc) : base(dialogue)
    {
        this.portrait = animalPortrait;
        this.portraitSourceRect = portraitSourceRect;
        this.npc = npc;
        this.friendshipJewel = Rectangle.Empty;

    }

    public override void draw(SpriteBatch b)
    {
        base.draw(b);

        if (!this.transitioning)
            base.drawPortrait(b);
    }
}
