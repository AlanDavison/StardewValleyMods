using System.Collections.Generic;

namespace VAF.Models;

public class VoLines : List<VoLine>
{
    private List<VoLine> voLines;
    private string assetName;

    public List<VoLine> Lines
    {
        get => this.voLines;
    }

    public bool ContainsDialogueKey(string key)
    {
        foreach (VoLine line in this)
        {
            if (line.IsForDialogueKey(key))
                return true;
        }

        return false;
    }

    public bool TryGetFilePath(string key, out string filePath)
    {
        filePath = "";

        foreach (VoLine line in this)
        {
            if (line.IsForDialogueKey(key))
            {
                filePath = line.GetFilePath();

                return true;
            }
        }

        return false;
    }

    public VoLines(string assetName)
    {
        this.voLines = new List<VoLine>();
        this.assetName = assetName;
    }

    public string GetAssetName()
    {
        return this.assetName;
    }

    public override string ToString()
    {
        return this.assetName;
    }

    // public void Add(VoLine line)
    // {
    //     this.voLines.Add(line);
    // }
    //
    // public void Remove(VoLine line)
    // {
    //     this.voLines.Remove(line);
    // }
}
