using System.Collections.Generic;
using Newtonsoft.Json;

namespace VAFPackFormat.Models
{
    public class CharacterVo
    {
        [JsonProperty("Format")] public string FormatVersion;
        public string Directory;
        public int Priority;
        public string Character { get; set; }
        public List<Voiceover> Voiceover;
    }
}
