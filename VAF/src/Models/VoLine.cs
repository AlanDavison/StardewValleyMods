using Microsoft.Xna.Framework.Audio;
using StardewModdingAPI;

namespace VAF.Models;

public class VoLine
{
    private IAssetName dialogueAssetName;
    private string key;
    private string filePath;
    private IManifest owningMod;
    private CueDefinition cue;

    public VoLine(IAssetName dialogueAssetName, string key, string filePath, IManifest owningMod)
    {
        this.dialogueAssetName = dialogueAssetName;
        this.key = key;
        this.filePath = filePath;
        this.owningMod = owningMod;
    }
}
