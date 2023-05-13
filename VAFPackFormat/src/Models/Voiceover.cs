using System.Collections.Generic;
using Newtonsoft.Json;

namespace VAFPackFormat.Models;

public class Voiceover
{
    [JsonProperty("Dialogue Path")] public string DialoguePath { get; set; }

    [JsonProperty("VO Files")] public List<VoFile> VoFiles { get; set; }
}
