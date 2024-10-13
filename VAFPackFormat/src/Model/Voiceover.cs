using System.Collections.Generic;
using Newtonsoft.Json;

namespace VAFPackFormat.Model;

public class Voiceover
{
    [JsonProperty("Dialogue Path")] private string dialoguePath;
    [JsonProperty("VO Files")] private List<VoFile> voFiles;

    public string DialoguePath => this.dialoguePath;
    public List<VoFile> VoFiles => this.voFiles;
}
