using System;
using System.Reflection;
using StardewValley.GameData.Buffs;

namespace TemplateProject.Helpers;

public class BuffAttributeDataHelper
{
    public static BuffAttributesData Add(BuffAttributesData data1, BuffAttributesData data2)
    {
        BuffAttributesData addedData = new BuffAttributesData();

        addedData.CombatLevel = MathF.Round(data1.CombatLevel + data2.CombatLevel, 2);
        addedData.FarmingLevel = MathF.Round(data1.FarmingLevel + data2.FarmingLevel, 2);
        addedData.FishingLevel = MathF.Round(data1.FishingLevel + data2.FishingLevel, 2);
        addedData.MiningLevel = MathF.Round(data1.MiningLevel + data2.MiningLevel, 2);
        addedData.LuckLevel = MathF.Round(data1.LuckLevel + data2.LuckLevel, 2);
        addedData.ForagingLevel = MathF.Round(data1.ForagingLevel + data2.ForagingLevel, 2);
        addedData.MaxStamina = MathF.Round(data1.MaxStamina + data2.MaxStamina, 2);
        addedData.MagneticRadius = MathF.Round(data1.MagneticRadius + data2.MagneticRadius, 2);
        addedData.Speed = MathF.Round(data1.Speed + data2.Speed, 2);
        addedData.Defense = MathF.Round(data1.Defense + data2.Defense, 2);
        addedData.Attack = MathF.Round(data1.Attack + data2.Attack, 2);
        addedData.Immunity = MathF.Round(data1.Immunity + data2.Immunity, 2);
        addedData.AttackMultiplier = MathF.Round(data1.AttackMultiplier + data2.AttackMultiplier, 2);
        addedData.KnockbackMultiplier = MathF.Round(data1.KnockbackMultiplier + data2.KnockbackMultiplier, 2);
        addedData.WeaponSpeedMultiplier = MathF.Round(data1.WeaponSpeedMultiplier + data2.WeaponSpeedMultiplier, 2);
        addedData.CriticalChanceMultiplier = MathF.Round(data1.CriticalChanceMultiplier + data2.CriticalChanceMultiplier, 2);
        addedData.CriticalPowerMultiplier = MathF.Round(data1.CriticalPowerMultiplier + data2.CriticalPowerMultiplier, 2);
        addedData.WeaponPrecisionMultiplier = MathF.Round(data1.WeaponPrecisionMultiplier + data2.WeaponPrecisionMultiplier, 2);

        return addedData;
    }

    public static BuffAttributesData? AddBuffStat(string stat, BuffAttributesData? data, float val)
    {
        try
        {
            data?.GetType()?.GetField(stat)?.SetValue(data, val);
        }
        catch (Exception e) when (e is ArgumentNullException ||
                                  e is NotSupportedException ||
                                  e is FieldAccessException ||
                                  e is TargetException ||
                                  e is ArgumentException)
        {
            return data;
        }

        return data;
    }
}
