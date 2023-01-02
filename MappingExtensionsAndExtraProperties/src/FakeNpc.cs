using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Xna.Framework;
using StardewValley;

namespace MappingExtensionsAndExtraProperties;

public class FakeNpc : NPC
{
    private List<string> dialogueLines;

    public FakeNpc(AnimatedSprite sprite, Vector2 tile, int facingDirection, string name)
        : base(sprite, tile, facingDirection, name)
    {
    }

    // public override bool checkAction(Farmer who, GameLocation l)
    // {
    //     // this.CurrentDialogue.Push(new Dialogue("Hey @, you think you could build a raft?#$e#I saw a few cool islands on the way here I want to visit.", this));
    //     //
    //     // Game1.drawDialogue(this);
    //     // base.facePlayer(who);
    //
    //     return base.checkAction(who, l);
    // }

    public override bool CanSocialize
    {
        get
        {
            return false;
        }
    }

    public override bool canTalk()
    {
        return true;
    }
}
