using System;
using System.Collections.Generic;
using DecidedlyShared.Logging;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;

namespace MappingExtensionsAndExtraProperties;

public class FakeNpc : NPC
{
    private List<string> dialogueLines;
    private Logger logger;
    private GameLocation npcLocation;

    public FakeNpc(AnimatedSprite sprite, Vector2 tile, int facingDirection, string name, Logger logger, GameLocation npcLocation)
        : base(sprite, tile, facingDirection, name)
    {
        this.logger = logger;
        this.npcLocation = npcLocation;
        // this.logger.Log($"{name} of type {nameof(FakeNpc)} created.", LogLevel.Trace);
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

    public void KillNpc()
    {
        if (this.npcLocation != null)
        {
            if (this.npcLocation.characters.Contains(this))
            {
                this.npcLocation.characters.Remove(this);
            }
        }

        this.logger?.Log($"{this.name.Value} killed in location {this.npcLocation.Name}.", LogLevel.Trace);
    }
}
