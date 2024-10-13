using System.Collections.Generic;
using Newtonsoft.Json;

namespace VAFPackFormat.Model;

public class CharacterVo
{
    [JsonProperty("Format")] private string formatVersion;
    private string directory;
    private int priority;
    private string character;
    private List<Voiceover> voiceover;

    public string FormatVersion => this.formatVersion;
    public string Directory => this.directory;
    public int Priority => this.priority;
    public string Character => this.character;
    public List<Voiceover> Voiceover => this.voiceover;
}
