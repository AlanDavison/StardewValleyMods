using System;
using System.Collections.Generic;
using System.Linq;

namespace DecidedlyShared.Utilities;

public class General
{
    public static bool TryCombineDictionaryPairs(Dictionary<string, string> inputDict, out string[]? mergedPairs)
    {
        mergedPairs = null;

        try
        {
            mergedPairs =
                inputDict.Select<KeyValuePair<string, string>, string>(pair => { return $"{pair.Key} {pair.Value}"; }).ToArray();

            return true;
        }
        catch (Exception e)
        {
            return false;
        }
    }
}

