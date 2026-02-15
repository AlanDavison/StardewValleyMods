namespace PlantingByBirb.Framework.Model;

public class BirbSeed
{
    public string SeedId { get; private set; }
    public int PlantingWeight { get; init; }

    public override int GetHashCode()
    {
        return this.SeedId.GetHashCode();
    }

    public void SetSeedId(string id)
    {
        this.SeedId = id;
    }

    public virtual bool Equals(BirbSeed? other)
    {
        return this.GetHashCode() == other?.GetHashCode();
    }
}
