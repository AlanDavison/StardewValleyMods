using StardewValley;
using StardewValley.Buffs;
using StardewValley.GameData.Buffs;
using TemplateProject.Helpers;

namespace BuffCrops.Framework;

public class BuffBuilder
{
    private BuffAttributesData buffData = new();

    public BuffBuilder AddBuff(
        BuffAttributesData data)
    {
        this.buffData = BuffAttributeDataHelper.Add(this.buffData, data);

        return this;
    }

    public Buff Build()
    {
        return new Buff("DH.BuffCrops.Buff", effects: new BuffEffects(this.buffData));
    }
}
