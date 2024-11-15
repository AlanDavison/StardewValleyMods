using Microsoft.Xna.Framework.Graphics;
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

    public Buff Build(string displayName, string description, int duration, Texture2D buffTexture, int iconSheetIndex)
    {
        return new Buff(
            "DH.BuffCropsEvents.Buff",
            effects: new BuffEffects(this.buffData),
            displayName: displayName,
            description: description,
            duration: duration,
            iconTexture: buffTexture,
            iconSheetIndex:iconSheetIndex) {customFields = {}};
    }
}
