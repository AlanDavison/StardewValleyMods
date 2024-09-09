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
    private NPC npc;

    public AnimalDialogueBox(Dialogue dialogue, NPC npc) : base(dialogue)
    {
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
