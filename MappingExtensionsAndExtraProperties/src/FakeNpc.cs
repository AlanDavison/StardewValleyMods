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
    private string internalId;
    private int framesSinceJump = 0;

    /// <summary>
    /// Warning: This can be null if the NPC is serialised over the network.
    /// </summary>
    public string InternalId
    {
        get => this.internalId;
    }

    public FakeNpc(string interalId, AnimatedSprite sprite, Vector2 tile, int facingDirection, string name, Logger logger, GameLocation npcLocation)
        : base(sprite, tile, facingDirection, name)
    {
        this.internalId = interalId;
        this.logger = logger;
        this.npcLocation = npcLocation;
        base.currentLocation = this.npcLocation;
        this.logger.Log($"{name} of type {nameof(FakeNpc)} created in {npcLocation.Name}.", LogLevel.Trace);

#if DEBUG
        // Extra debug logging in case I need to try to narrow down serialisation issues.
        this.logger.Log("Players present:");

        foreach (Farmer player in npcLocation.farmers)
        {
            this.logger.Log($"{player.Name}:{player.userID}:{player.UniqueMultiplayerID.ToString()}", LogLevel.Info);
        }
#endif
    }

    public override void update(GameTime time, GameLocation location)
    {
        this.framesSinceJump++;

        if (this.framesSinceJump > 40)
        {
            base.jump(4f);
            base.yJumpGravity = -0.5f;
            this.framesSinceJump = 0;
            this.logger.Log("Jumped.", LogLevel.Info);
        }

        base.update(time, location);
    }

    public FakeNpc()
    {

    }

    // Vanilla method does centre the shadow correctly.
    // public override Vector2 GetShadowOffset()
    // {
    //     int npcWidth = this.sprite.Value.SpriteWidth;
    //     int npcHeight = this.sprite.Value.SpriteHeight;
    //
    //     return new Vector2(0, 0);
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
        else
            this.logger?.Error($"{this.Name}'s internal FakeNPC location was null. Please let me know if you see this occur!");

        this.logger?.Log($"{this.Name} killed in location {this.npcLocation.Name}.", LogLevel.Trace);
    }
}
