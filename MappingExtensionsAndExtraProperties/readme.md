# Mapping Extensions and Extra Properties (MEEP) Documentation
**Version 2.4.0 -- The update that gives modders the ability to make their custom trees invulnerable, give their spawned farm animals portraits, and spawn coloured slimes in events.**

All releases can be found on my [Nexus page](https://www.nexusmods.com/users/79440738?tab=user+files).

## What it does
This mod does nothing on its own. Its primary purpose is to allow map authors to spice up their maps with the new custom tile properties, extra features, etc., that this mod adds.

## Current features
Click on the link to go to the mini-docs for each one

| Updated in | **Detailed Description**                                                                                     | **Layer** | **Description**                                                                                                                                                                                                                                                                                                                                                                                      |
|:-----------|--------------------------------------------------------------------------------------------------------------|-----------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| 2.0.0      | [*Closeup Interaction*](docs/tile-properties.md#Using-the-CloseupInteraction-tile-properties)                | Buildings | This tile property will display a specified image on the screen when the player interacts with the tile it's placed on. If you want the player to be able to examine a photo on a desk and actually see the photo up-close, this is the one to use.                                                                                                                                                  |
| 2.0.0      | [*Closeup Interaction Text*](docs/tile-properties.md#Using-the-CloseupInteraction-tile-properties)           | Buildings | This tile property only works in conjunction with `CloseupInteraction_Image`, and will display the specified text as a description below the image.                                                                                                                                                                                                                                                  |
| 2.0.0      | [*Closeup Interaction Reel*](docs/tile-properties.md#Using-the-CloseupInteraction-reel-tile-properties)      | Buildings | This is a special variation of the closeup interaction properties. With this method, the mod will display the first image, and allow the player to also look at image 2, image 3, etc., all while allowing you to optionally have a text description for required images.                                                                                                                            |
| 2.0.0      | [*Closeup Interaction Sound*](docs/tile-properties.md#Using-the-MEEP_CloseupInteraction_Sound-tile-property) | Buildings | This is a tile property you can use alongside any of the other Closeup Interaction properties. When you specify a game sound cue using it, the sound will play when the player opens the interaction, or turns the page in the case of a reel.                                                                                                                                                       |
| 2.0.0      | [*Set Mail Flag*](docs/tile-properties.md#Using-the-MEEP_SetMailFlag-tile-property)                          | Buildings | This tile property will set the specified mail flag when the player interacts with the tile it's on.                                                                                                                                                                                                                                                                                                 |
| 2.2.0      | [Adding a conversation topic](docs/tile-properties.md#Using-the-MEEP_AddConversationTopic-tile-property)     | Buildings | MEEP allows you to add a conversation topic to the player that interacts with a tile. This works alongside any other MEEP tile properties. For instance, you can set a conversation topic when the player clicks to look at something on an NPC's table.                                                                                                                                             |
| 2.0.0      | [*Fake NPC*](docs/tile-properties.md#Using-the-MEEP_FakeNPC-tile-property)                                   | Back      | This tile property will spawn a fake NPC on the tile it's placed on. This NPC will breathe like a normal NPC, face you like a normal NPC, and can be talked to like a normal NPC. You can also specify a custom sprite size for the NPC. For example: a 32x32 NPC, or a 64x64 NPC. Other sizes may work, but haven't been tested.                                                                    |
| 2.0.0      | [*Letter*](docs/tile-properties.md#Using-the-MEEP-Letter-tile-property)                                      | Buildings | With the Letter tile properties, you can trigger a vanilla-style letter/mail when the player interacts with the specified tile.                                                                                                                                                                                                                                                                      |
| 2.0.0      | [*Letter Type*](docs/tile-properties.md#MEEP_Letter_Type)                                                    | Buildings | This property allows you to specify a vanilla letter background, *or* a custom letter background image.                                                                                                                                                                                                                                                                                              |
| 2.4.0      | [Farm Animal Spawning](docs/non-map-properties.md#Spawning-farm-animals)                                     | N/A       | MEEP lets you spawn any farm animal in the game (including custom ones added with 1.6's new custom farm animal feature) on a map of your choosing. Farm animals spawned by MEEP can't be milked/sheared/sold, and can display a custom message when you chat with them. Since 2.4.0, this now lets you specify a portrait image for your farm animals to give them dialogue similar to a normal NPC. |
| **2.4.0**  | [Invulnerable Trees](docs/non-map-properties.md#Making-trees-invulnerable)                                   | N/A       | This lets you mark specific types of custom trees as being indestructible so the player can't chop them down. Good for making decorative custom trees.                                                                                                                                                                                                                                               |
| **2.4.0** | [Coloured slime event command](docs/non-map-properties.md#Spawning-coloured-slimes-in-events)                | N/A       | This is a new event command, which will let you add custom coloured slimes to your events.                                                                                                                                                                                                                                                                                                           |


## Using the features
Using the features is fairly simple. There are a few things you'll need to know that I won't be covering here:
1) The basics of creating a Content Patcher pack. See [the Content Patcher docs](https://github.com/Pathoschild/StardewMods/blob/develop/ContentPatcher/docs/author-guide.md).
2) How to load an image asset using Content Patcher. See [documentation for the `Load` action](https://github.com/Pathoschild/StardewMods/blob/develop/ContentPatcher/docs/author-guide/action-load.md).
3) How to patch tile properties using Content Patcher (see [documentation for the `EditMap` action](https://github.com/Pathoschild/StardewMods/blob/develop/ContentPatcher/docs/author-guide/action-editmap.md#edit-map-tiles)), or how to add tile properties to your map [directly using Tiled here](https://stardewvalleywiki.com/Modding:Maps#Tile_properties).
4) How to patch data models using Content Patcher (see [the documentation for the `EditData` action here](https://github.com/Pathoschild/StardewMods/blob/develop/ContentPatcher/docs/author-guide/action-editdata.md)).
5) Add the appropriate keys to your mod's manifest to tell MEEP you're using certain features (see the [first section here](#Adding-meep-feature-keys-to-your-manifest)).

### Adding MEEP feature keys to your manifest
This part is simple, but important. In order for MEEP to know which features to enable, it needs to know which feature every mod using it requires, and which version of MEEP the mod is expecting. Event commands will always be added, as they have no potential for side effects.

All the current keys are as follows:

* `DH.MEEP` (Mandatory to use MEEP.)
* `DH.MEEP.CloseupInteractions`
* `DH.MEEP.FakeNPCs`
* `DH.MEEP.VanillaLetters`
* `DH.MEEP.SetMailFlag`
* `DH.MEEP.AddConversationTopic`
* `DH.MEEP.FarmAnimalSpawns`
* `DH.MEEP.InvulnerableTrees`

The following example uses closeup interactions, fake NPCs, and was made for version 2.0.0 of MEEP.

```json
{
    "Name": "Tile Property Test Mod",
    "Author": "DecidedlyHuman",
    "Version": "1.0.0",
    "Description": "Tile properties for testing.",
    "UniqueID": "DecidedlyHuman.TilePropertyTestMod",
    "UpdateKeys": [],
    "ContentPackFor": {
        "UniqueID": "Pathoschild.ContentPatcher"
    },
    "DH.MEEP": "2.0.0",
    "DH.MEEP.CloseupInteractions": "",
    "DH.MEEP.FakeNPCs": ""
}
```

This next example uses only the letter and set mail flag properties.
```json
{
    "Name": "Tile Property Test Mod",
    "Author": "DecidedlyHuman",
    "Version": "1.0.0",
    "Description": "Tile properties for testing.",
    "UniqueID": "DecidedlyHuman.TilePropertyTestMod",
    "UpdateKeys": [],
    "ContentPackFor": {
        "UniqueID": "Pathoschild.ContentPatcher"
    },
    "DH.MEEP": "2.0.0",
    "DH.MEEP.VanillaLetters": "",
    "DH.MEEP.SetMailFlag": ""
}
```
