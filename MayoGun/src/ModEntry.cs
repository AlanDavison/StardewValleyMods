using System;
using DecidedlyShared.APIs;
using DecidedlyShared.Logging;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Monsters;

namespace MayoGun;

public class ModEntry : Mod
{
    private Logger logger;
    private IModHelper helper;
    private IArcheryApi? archery;

    public override void Entry(IModHelper helper)
    {
        this.helper = helper;
        this.logger = new Logger(this.Monitor, this.helper.Translation);
        helper.Events.GameLoop.GameLaunched += this.GameLoopOnGameLaunched;
        helper.Events.GameLoop.UpdateTicked += this.GameLoopUpdateTicked;
        MayoPatches.InitialisePatches(helper, this.Monitor, this.logger);

        var harmony = new Harmony(this.ModManifest.UniqueID);

        // harmony.Patch(
        //     AccessTools.Method(typeof(Monster), nameof(Monster.update),
        //         new Type[] {typeof(GameTime), typeof(GameLocation)}),
        //     prefix: new HarmonyMethod(typeof(MayoPatches), nameof(MayoPatches.MonsterUpdate_Prefix)));

        // harmony.Patch(
        //     AccessTools.Method(typeof(Character), nameof(Character.draw),
        //         new Type[] {typeof(SpriteBatch), typeof(int), typeof(float)}),
        //     postfix: new HarmonyMethod(typeof(MayoPatches), nameof(MayoPatches.CharacterDraw_Postfix)));

        harmony.Patch(
            AccessTools.Method(typeof(Character), nameof(Character.update),
                new Type[] {typeof(GameTime), typeof(GameLocation), typeof(long), typeof(bool)}),
            prefix: new HarmonyMethod(typeof(MayoPatches), nameof(MayoPatches.CharacterUpdate_Prefix)));
    }

    private void GameLoopUpdateTicked(object? sender, UpdateTickedEventArgs e)
    {
        MayoCore.TickMonsters();
    }

    private void GameLoopOnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        string archeryId = "PeacefulEnd.Archery";

        // If Archery is loaded...
        if (this.helper.ModRegistry.IsLoaded(archeryId))
        {
            // ...grab its API if the version is correct.
            if (!this.helper.ModRegistry.Get(archeryId)!.Manifest.Version.IsOlderThan(new SemanticVersion(2, 2, 5)))
            {
                this.archery = this.helper.ModRegistry.GetApi<IArcheryApi>(archeryId);
                this.archery!.OnAmmoHitMonster += this.ArcheryOnOnAmmoHitMonster;
            }
        }
    }

    private void ArcheryOnOnAmmoHitMonster(object? sender, IAmmoHitMonsterEventArgs e)
    {
        if (e.AmmoId.Equals("DecidedlyHuman.MayoGun.ArcheryPack/Arrow/DecidedlyHuman.MayoGun.Ammo"))
        {
            MayoCore.GlobMonsterWithMayo(e.Monster);
        }
    }
}
