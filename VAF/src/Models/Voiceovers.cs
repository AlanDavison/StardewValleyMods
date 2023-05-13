using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Security.Permissions;

namespace VAF.Models;

public class Voiceovers : Dictionary<string, VoLines>
{
    public bool ContainsVoiceover(string assetName, string key)
    {
        if (this.ContainsKey(assetName))
        {
            if (this[assetName].ContainsDialogueKey(key))
            {
                return true;
            }
        }

        return false;
    }

    public bool TryGetFileForDialogue(string assetName, string key, out string filePath)
    {
        filePath = "";

        try
        {
            if (this[assetName].ContainsDialogueKey(key))
            {
                if (this[assetName].TryGetFilePath(key, out filePath))
                {
                    return true;
                }
            }
        }
        catch (Exception e)
        {
            return false;
        }

        return false;
    }
}
