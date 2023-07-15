using StardewValley.Monsters;

namespace MayoGun;

public class MonsterData
{
    private Monster monster;
    private int ticksRemaining;
    private int originalSpeed;

    public Monster Monster
    {
        get
        {
            return this.monster;
        }
    }

    public int TicksRemaining
    {
        get
        {
            return this.ticksRemaining;
        }
        set
        {
            this.ticksRemaining = value;
        }
    }

    public int OriginalSpeed { get; }

    public MonsterData(Monster monster, int ticks)
    {
        this.monster = monster;
        this.originalSpeed = monster.Speed;
        this.ticksRemaining = ticks;
    }


    public bool IsForMonster(Monster m)
    {
        return m == this.monster;
    }

    // public override bool Equals(object? obj)
    // {
    //     return ReferenceEquals(obj, this.monster);
    // }
}
