using Microsoft.Xna.Framework.Audio;
using Newtonsoft.Json;

namespace VAFPackFormat.Model;

public class VoFile
{
    [JsonProperty("Key")] private string key;
    [JsonProperty("File")] private string file;
    [JsonIgnore] private CueDefinition? cue;

    public string Key => this.key;
    public string File => this.file;
    public CueDefinition? Cue => this.cue;
}
