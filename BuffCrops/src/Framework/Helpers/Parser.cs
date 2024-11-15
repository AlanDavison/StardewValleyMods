using System;
using System.Collections.Generic;
using DecidedlyShared.Logging;
using StardewValley.GameData.Buffs;
using TemplateProject.Helpers;

namespace BuffCrops.Framework.Helpers;

public class Parser
{
    private Logger logger;

    public Parser(Logger logger)
    {
        this.logger = logger;
    }

    public bool TryGetSpaceCoreSkillData(string property, out Dictionary<string, float> skillContribution)
    {
        skillContribution = new Dictionary<string, float>();

        try
        {
            string[] splitProperty = property.Split(" ");

            /*
            For this property, we expect an even amount of arguments: skillid amount skillid amount skillid amount
            */

            if (splitProperty.Length % 2 != 0)
                return false;

            for (int i = 1; i < splitProperty.Length; i += 2)
            {
                if (!float.TryParse(splitProperty[i], out float _))
                    return false;
            }

            for (int i = 0; i < splitProperty.Length; i += 2)
            {
                if (i < splitProperty.Length)
                {
                    if (skillContribution.ContainsKey(splitProperty[i]))
                        skillContribution[splitProperty[i]] = skillContribution[splitProperty[i]] += float.Parse(splitProperty[i + 1]);
                    else
                        skillContribution.Add(splitProperty[i], float.Parse(splitProperty[i + 1]));
                }
            }

            return true;
        }
        catch (Exception e)
        {
            this.logger.Exception(e);
            return false;
        }
    }

    public bool TryGetBuffAttributesData(string property, out BuffAttributesData? parsedProperty)
    {
        parsedProperty = new BuffAttributesData();

        try
        {
            string[] splitProperty = property.Split(" ");

            /*
            For this property, we expect an even amount of arguments: buffname amount buffname amount buffname amount
            */

            if (splitProperty.Length % 2 != 0)
                return false;

            for (int i = 0; i < splitProperty.Length; i += 2)
            {
                if (!ValidBuffStats.Stats.Contains(splitProperty[i]))
                    return false;
            }

            // Now we've verified all the buff stat names are valid, we do the same for the modifiers.

            for (int i = 1; i < splitProperty.Length; i += 2)
            {
                if (!float.TryParse(splitProperty[i], out float _))
                    return false;
            }

            for (int i = 0; i < splitProperty.Length; i += 2)
            {
                if (i < splitProperty.Length)
                {
                    parsedProperty = BuffAttributeDataHelper.AddBuffStat(
                        splitProperty[i],
                        parsedProperty,
                        float.Parse(splitProperty[i + 1]));
                }
            }

            return true;
        }
        catch (Exception e)
        {
            this.logger.Exception(e);
            return false;
        }
    }
}
