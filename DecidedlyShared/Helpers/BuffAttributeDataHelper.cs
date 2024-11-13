using System;
using System.Reflection;
using StardewValley.GameData.Buffs;

namespace TemplateProject.Helpers;

public class BuffAttributeDataHelper
{
    public static BuffAttributesData Add(BuffAttributesData data1, BuffAttributesData data2)
    {
        BuffAttributesData addedData = new BuffAttributesData();

        addedData.CombatLevel = data1.CombatLevel + data2.CombatLevel;
        addedData.FarmingLevel = data1.FarmingLevel + data2.FarmingLevel;
        addedData.FishingLevel = data1.FishingLevel + data2.FishingLevel;
        addedData.MiningLevel = data1.MiningLevel + data2.MiningLevel;
        addedData.LuckLevel = data1.LuckLevel + data2.LuckLevel;
        addedData.ForagingLevel = data1.ForagingLevel + data2.ForagingLevel;
        addedData.MaxStamina = data1.MaxStamina + data2.MaxStamina;
        addedData.MagneticRadius = data1.MagneticRadius + data2.MagneticRadius;
        addedData.Speed = data1.Speed + data2.Speed;
        addedData.Defense = data1.Defense + data2.Defense;
        addedData.Attack = data1.Attack + data2.Attack;
        addedData.Immunity = data1.Immunity + data2.Immunity;
        addedData.AttackMultiplier = data1.AttackMultiplier + data2.AttackMultiplier;
        addedData.KnockbackMultiplier = data1.KnockbackMultiplier + data2.KnockbackMultiplier;
        addedData.WeaponSpeedMultiplier = data1.WeaponSpeedMultiplier + data2.WeaponSpeedMultiplier;
        addedData.CriticalChanceMultiplier = data1.CriticalChanceMultiplier + data2.CriticalChanceMultiplier;
        addedData.CriticalPowerMultiplier = data1.CriticalPowerMultiplier + data2.CriticalPowerMultiplier;
        addedData.WeaponPrecisionMultiplier = data1.WeaponPrecisionMultiplier + data2.WeaponPrecisionMultiplier;

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
