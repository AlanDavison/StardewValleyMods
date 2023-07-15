using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using StardewValley.Monsters;

namespace MayoGun;

public static class MayoCore
{
    private static List<MonsterData> monsters = new List<MonsterData>();

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
