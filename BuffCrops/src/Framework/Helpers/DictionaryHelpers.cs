using System.Collections.Generic;
using System.Linq;

namespace BuffCrops.Framework.Helpers;

public class DictionaryHelpers
{
    public static Dictionary<string, float> AddDictionaries(Dictionary<string, float> first, Dictionary<string, float> second)
    {
        Dictionary<string, float> addedDict = new Dictionary<string, float>();

        foreach (var kvp in first)
        {
            addedDict.Add(kvp.Key, kvp.Value);
        }

        foreach (var kvp in second)
        {
            if (addedDict.ContainsKey(kvp.Key))
                addedDict[kvp.Key] = kvp.Value + addedDict[kvp.Key];
            else
                addedDict.Add(kvp.Key, kvp.Value);
        }

        return addedDict;
    }
}

