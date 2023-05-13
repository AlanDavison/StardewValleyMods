using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Channels;
using Microsoft.CodeAnalysis;
using StardewModdingAPI;
using VAFPackFormat.Models;

namespace VAF.Models;

public class Pack
{
    private List<CharacterVo> characterVoiceovers = new List<CharacterVo>();
    private string packDirectory;
    private IManifest owningMod;

    public List<CharacterVo> CharacterVoiceovers
    {
        get => this.characterVoiceovers;
    }

    public bool IsEmpty
    {
        get { return !this.characterVoiceovers.Any(); }
    }

    public string PackDirectory
    {
        get => this.packDirectory;
    }

    public IManifest OwningMod
    {
        get => this.owningMod;
    }

    public void AddCharacterVo(CharacterVo characterVoiceover, string packDirectory, IManifest owningMod)
    {
        this.characterVoiceovers.Add(characterVoiceover);
        this.packDirectory = packDirectory;
        this.owningMod = owningMod;
    }
}
