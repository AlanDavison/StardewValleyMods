using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Monsters;

namespace MayoGun;

public static class MayoCore
{
    private static List<MonsterData> monsters = new List<MonsterData>();
    private static Texture2D? mayoGlob = null;

    public static void GlobMonsterWithMayo(Monster monster)
    {
        monsters.Add(new MonsterData(monster, 600));
        // monster.Speed = 0;
        // monster.speed = 0;
    }

    public static void TickMonsters()
    {
        if (monsters.Count == 0)
            return;

        List<MonsterData> toRemove = new List<MonsterData>();

        foreach (var globbedMonster in monsters)
        {
            globbedMonster.TicksRemaining -= 1;
            if (globbedMonster.TicksRemaining <= 0)
            {
                if (globbedMonster.Monster is null)
                {
                    toRemove.Add(globbedMonster);
                    continue;
                }

                // globbedMonster.Monster.Speed = globbedMonster.OriginalSpeed;
                // globbedMonster.Monster.speed = globbedMonster.OriginalSpeed;
                toRemove.Add(globbedMonster);
            }
        }

        if (toRemove.Count == 0)
            return;

        foreach (var monster in toRemove)
        {
            monsters.Remove(monster);
        }
    }

    public static void DrawTimerIfNecessary(SpriteBatch sb)
    {
        if (mayoGlob is null)
        {
            mayoGlob = Game1.content.Load<Texture2D>("DecidedlyHuman/MayoLauncher/MayoGlob");

            return;
        }

        foreach (MonsterData monsterData in MayoCore.monsters)
        {
            if (Game1.currentLocation.characters.Contains(monsterData.Monster))
            {
                // Vector2 position = monsterData.Monster.Position;

                Vector2 position = monsterData.Monster.getLocalPosition(Game1.viewport);
                Vector2 timerOffsetPosition = position + new Vector2(0, -96);
                // Vector2 mobOffsetPosition = position + new Vector2(0, -monsterData.Monster.Sprite.SpriteHeight * 2);
                // Vector2 position = new Vector2(
                //     (monsterData.Monster.Position.X * Game1.tileSize) - Game1.viewport.X,
                //     (monsterData.Monster.Position.Y * Game1.tileSize) - Game1.viewport.Y);
                // position += new Vector2(0, -32);
                // position = new Vector2((position.X * Game1.tileSize) - Game1.viewport.X,
                //     (position.Y * Game1.tileSize) - Game1.viewport.Y);

                int timeRemaining = MayoCore.GetGlobTimeRemaining(monsterData.Monster as Monster).Value / 60;

                DecidedlyShared.Utilities.Drawing.DrawStringWithShadow(
                    sb,
                    Game1.dialogueFont,
                    timeRemaining.ToString(),
                    timerOffsetPosition,
                    new Color(0, 133, 255),
                    new Color(0, 70, 170));

                // This is a mayo-loving nightmare, because mob sizes and bounding boxes and sprite sizes aren't consistent.
                // sb.Draw(mayoGlob, mobOffsetPosition, mayoGlob.Bounds, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
            }
        }
    }

    public static int? GetGlobTimeRemaining(Monster monster)
    {
        foreach (var m in monsters)
        {
            if (m.IsForMonster(monster))
                return m.TicksRemaining;
        }

        return null;
    }

    public static bool IsMonsterGlobbed(Monster monster)
    {
        foreach (var m in monsters)
        {
            if (m.IsForMonster(monster))
                return m.TicksRemaining > 0;
        }

        return false;
    }
}
