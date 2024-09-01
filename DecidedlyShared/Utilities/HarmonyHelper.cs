using System;
using System.Reflection;
using DecidedlyShared.Models;
using HarmonyLib;
using StardewValley;

namespace DecidedlyShared.Utilities;

public class HarmonyHelper
{
    public static DescriptiveBool TryPrefixPatchMethod(Harmony harmony, Type patchesClass, string methodName)
    {
        try
        {
            MethodInfo? prefixMethod = patchesClass.GetMethod(methodName);

            if (prefixMethod is not null)
            {
                harmony.Patch(
                    AccessTools.Method(typeof(Tool), nameof(Tool.DoFunction)),
                    prefix: new HarmonyMethod(prefixMethod));

                return new DescriptiveBool(true);
            }
        }
        catch (Exception e) when (e is ArgumentNullException || e is AmbiguousMatchException)
        {
            return new DescriptiveBool(false, methodName, "Caught exception.", e);
        }

        return new DescriptiveBool(false, methodName, $"Unknown error patching method {methodName} in class {patchesClass.AssemblyQualifiedName}");
    }

    public static DescriptiveBool TryPostfixPatchMethod(Harmony harmony, Type patchesClass, string methodName)
    {
        try
        {
            MethodInfo? prefixMethod = patchesClass.GetMethod(methodName);

            if (prefixMethod is not null)
            {
                harmony.Patch(
                    AccessTools.Method(typeof(Tool), nameof(Tool.DoFunction)),
                    postfix: new HarmonyMethod(prefixMethod));

                return new DescriptiveBool(true);
            }
        }
        catch (Exception e) when (e is ArgumentNullException || e is AmbiguousMatchException)
        {
            return new DescriptiveBool(false, methodName, "Caught exception.", e);
        }

        return new DescriptiveBool(false, methodName, $"Unknown error patching method {methodName} in class {patchesClass.AssemblyQualifiedName}");
    }
}
