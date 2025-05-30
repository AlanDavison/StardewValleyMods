using System;
using System.Collections.Generic;
using DecidedlyShared.APIs;
using StardewModdingAPI;

namespace DecidedlyShared.Helpers;

public class ApiHelper
{
    private static Dictionary<string, IInterfaceCore> initialisedInterfaces = new Dictionary<string, IInterfaceCore>();

    public static T? GetApi<T>(string uniqueId, Predicate<IModInfo> requirements, IModHelper helper) where T : class, IInterfaceCore
    {
        IModInfo? modInfo = helper.ModRegistry.Get(uniqueId);

        if (modInfo is null)
            return null;

        if (!requirements.Invoke(modInfo))
            return null;

        if (initialisedInterfaces.ContainsKey(uniqueId))
            return (T)initialisedInterfaces[uniqueId];

        T? newApi = helper.ModRegistry.GetApi<T>(uniqueId);

        if (newApi is null)
            return null;

        initialisedInterfaces.Add(uniqueId, newApi);
        return newApi;
    }
}
