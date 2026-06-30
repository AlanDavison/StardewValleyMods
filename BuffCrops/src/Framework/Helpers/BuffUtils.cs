using System;
using System.Text;
using StardewValley.GameData.Buffs;

namespace BuffCrops.src.Framework.Helpers;

public static class BuffUtils
{
    public static object GetBuffInformation(BuffAttributesData data)
    {
        StringBuilder builder = new StringBuilder();

        if (data.CombatLevel > 0f) builder.Append($"CombatLevel: {data.CombatLevel} ");
        if (data.FarmingLevel > 0f) builder.Append($"FarmingLevel: {data.FarmingLevel} ");
        if (data.FishingLevel > 0f) builder.Append($"FishingLevel: {data.FishingLevel} ");
        if (data.MiningLevel > 0f) builder.Append($"MiningLevel: {data.MiningLevel} ");
        if (data.LuckLevel > 0f) builder.Append($"LuckLevel: {data.LuckLevel} ");
        if (data.ForagingLevel > 0f) builder.Append($"ForagingLevel: {data.ForagingLevel} ");
        if (data.MaxStamina > 0f) builder.Append($"MaxStamina: {data.MaxStamina} ");
        if (data.MagneticRadius > 0f) builder.Append($"MagneticRadius: {data.MagneticRadius} ");
        if (data.Speed > 0f) builder.Append($"Speed: {data.Speed} ");
        if (data.Defense > 0f) builder.Append($"Defense: {data.Defense} ");
        if (data.Attack > 0f) builder.Append($"Attack: {data.Attack} ");
        if (data.AttackMultiplier > 0f) builder.Append($"AttackMultiplier: {data.AttackMultiplier} ");
        if (data.Immunity > 0f) builder.Append($"Immunity: {data.Immunity} ");
        if (data.KnockbackMultiplier > 0f) builder.Append($"KnockbackMultiplier: {data.KnockbackMultiplier} ");
        if (data.WeaponSpeedMultiplier > 0f) builder.Append($"WeaponSpeedMultiplier: {data.WeaponSpeedMultiplier} ");
        if (data.CriticalChanceMultiplier > 0f) builder.Append($"CriticalChanceMultiplier: {data.CriticalChanceMultiplier} ");
        if (data.CriticalPowerMultiplier > 0f) builder.Append($"CriticalPowerMultiplier: {data.CriticalPowerMultiplier} ");
        if (data.WeaponPrecisionMultiplier > 0f) builder.Append($"WeaponPrecisionMultiplier: {data.WeaponPrecisionMultiplier} ");

        return builder.ToString();
    }
}
