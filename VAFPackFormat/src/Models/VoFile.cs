using Microsoft.Xna.Framework.Audio;

namespace VAFPackFormat.Models;

public class VoFile
{
    public string Key = "";
    public string File { get; set; }
    public CueDefinition Cue { get; set; }
}
