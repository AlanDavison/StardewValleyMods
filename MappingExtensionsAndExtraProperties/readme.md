# Mapping Extensions and Extra Properties (MEEP) Documentation
**Version 1.0.0**

All releases can be found on my [Nexus page](https://www.nexusmods.com/users/79440738?tab=user+files).

## What it does
This mod does nothing on its own. Its primary purpose is to allow map authors to spice up their maps with the new custom tile properties, extra features, etc., that this mod adds.

## Current tile properties
### Click on the link to go to the mini-docs for each one
| **Tile Property**                                                                | **Layer** | **Description**                                                                                                                                                                                                                                     |
|----------------------------------------------------------------------------------|-----------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| [*MEEP_CloseupInteraction_Image*](#Using-the-CloseupInteraction-tile-properties) | Back      | This tile property will display a specified image on the screen when the player interacts with the tile it's placed on. If you want the player to be able to examine a photo on a desk and actually see the photo up-close, this is the one to use. |
| [*MEEP_CloseupInteraction_Text*](#Using-the-CloseupInteraction-tile-properties)  | Back      | This tile property only works in conjunction with `CloseupInteraction_Image`, and will display the specified text as a description below the image.                                                                                                 |
| [*MEEP_SetMailFlag*](#Using-the-MEEP_SetMailFlag-tile-property)                  | Back      | This tile property will set the specified mail flag when the player interacts with the tile it's on.                                                                                                                                                |
| [*MEEP_FakeNPC*](#Using-the-MEEP_FakeNPC-tile-property)                          | Back      | This tile property will spawn a fake NPC on the tile it's placed on. This NPC will breathe like a normal NPC, face you like a normal NPC, and can be talked to like a normal NPC.                                                                   |

## Using the tile properties
Using the tile properties is fairly simple. There are a few things you'll need to know that I won't be covering here:
1) The basics of creating a Content Patcher pack. See [here](https://github.com/Pathoschild/StardewMods/blob/develop/ContentPatcher/docs/author-guide.md).
2) How to load an image asset using Content Patcher. See [here](https://github.com/Pathoschild/StardewMods/blob/develop/ContentPatcher/docs/author-guide/action-load.md).
3) How to patch tile properties using Content Patcher (see [here](https://github.com/Pathoschild/StardewMods/blob/develop/ContentPatcher/docs/author-guide/action-editmap.md#edit-map-tiles)), or how to add tile properties to your map directly using Tiled (see [here](https://stardewvalleywiki.com/Modding:Maps#Tile_properties)).

### Using the CloseupInteraction tile properties
The basic format for `CloseupInteraction` is in the following snippet of an `EditMap` patch using Content Patcher.

```json
{
    "Format": "1.28.0",
    "Changes": [
        {
            // Loading Pierre's shop counter image
            "Action": "Load",
            "Target": "Mods/DecidedlyHuman/PierreCounterThing",
            "FromFile": "assets/pierre-counter-thing.png"
        },
        {
            // Apply the tile property
            "Action": "EditMap",
            "Target": "Maps/SeedShop",
            "MapTiles": [
                {
                    "Position": {
                        "X": 8,
                        "Y": 18
                    },
                    "Layer": "Back",
                    "SetProperties": {
                        "MEEP_CloseupInteraction_Image": "Mods/DecidedlyHuman/PierreCounterThing",
                        "MEEP_CloseupInteraction_Text": "I'm not even sure what this is... is it a plant pot, or a tomato?"
                    }
                }
            ]
        }
    ]
}
```
`MEEP_CloseupInteraction_Image` takes a few "arguments" in its tile property value. The first, and only mandatory one, is the asset name (which can be a built-in Stardew image). The second is the region of the specified image you want to be displayed.


For example...
```json
"MEEP_CloseupInteraction_Image": "LooseSprites/Cursors 540 305 42 28",
"MEEP_CloseupInteraction_Text": "The spirits tell me you're learning how to use a new mod..."
```
Will display the fortune teller, and a message reading "The spirits tell me you're learning how to use a new mod...".

In `540 305 42 28`, `540` is the x co-ordinate of the top-left corner of the region of the specified image you want to display, `305` is the y co-ordinate, `42` is the width, and `28` is the height.

**Warning**: It's worth keeping in mind the size of the image, and whether or not it will interfere with Stardew Valley when running at lower resolutions when combined with the text display option. I recommend you **always test your images at a varying UI scale settings and window sizes** if you want to play it safe.

### Using the MEEP_SetMailFlag tile property
This one is fairly self-explanatory. You would add the tile property `DHSetMailFlag`, and the value for it is the mail flag you want to be set. for example:

```json
{
    "Format": "1.28.0",
    "Changes": [
        {
            // Apply the tile property
            "Action": "EditMap",
            "Target": "Maps/SeedShop",
            "When": {
                "HasFlag |contains=DHSeenFortuneTellerImage": false
            },
            "MapTiles": [
                {
                    "Position": {
                        "X": 8,
                        "Y": 18
                    },
                    "Layer": "Back",
                    "SetProperties": {
                        "MEEP_CloseupInteraction_Image": "LooseSprites/Cursors 540 305 42 28",
                        "MEEP_CloseupInteraction_Text": "The spirits tell me you're learning how to use a new mod...",
                        "MEEP_SetMailFlag": "DHSeenFortuneTellerImage"
                    }
                }
            ]
        }
    ]
}
```

With this example, interacting with the tile will bring up the fortune teller image and message, and set the mail flag `DHSeenFortuneTellerImage`. Whenever Content Patcher refreshes its patches, the interaction to bring up the image and description will vanish. You can specify a custom update rate as seen [here](https://github.com/Pathoschild/StardewMods/blob/develop/ContentPatcher/docs/author-guide.md#how-often-are-patch-changes-applied).

You could also use this for any kind of conditional patch that checks for a mail flag.

### Using the MEEP_FakeNPC tile property
This tile property will allow you to spawn a "fake" NPC on a given tile. Unlike a "real" NPC, which needs a disposition, and lots of setup, a "fake" NPC needs very, very little. Fake NPCs cannot receive gifts, don't have a schedule, and won't move around. They're intended to be a middle ground between a simple static NPC sprite, and a fully-fledged NPC.

The basic setup is as follows:
```json
{
    "Format": "1.28.0",
    "Changes": [
        {
            "Action": "Load",
            "Target": "Portraits/NotAbigail",
            "FromFile": "assets/NotAbigail/NotAbigailPortrait.png"
        },
        {
            "Action": "Load",
            "Target": "Characters/NotAbigail",
            "FromFile": "assets/NotAbigail/NotAbigail.png"
        },
        {
            "Action": "EditData",
            "Target": "MEEP/FakeNPC/Dialogue/NotAbigail",
            "Entries": {
                "DialogueOne": "Hey @, you think you could build a raft?#$e#I saw a few cool islands on the way here I want to visit."
            }
        },
        {
            "Action": "EditMap",
            "Target": "Maps/Town"
            "MapTiles": [
                {
                    "Position": {
                        "X": 29,
                        "Y": 56
                    },
                    "Layer": "Back",
                    "SetProperties": {
                        "MEEP_FakeNPC": "NotAbigail"
                    }
                }
            ]
        }
    ]
}
```

Firstly, we need to load in a portrait for the NPC. You'll need to set the `FromFile` to point to wherever you have the source image.
```json
{
    "Action": "Load",
    "Target": "Portraits/NotAbigail",
    "FromFile": "assets/NotAbigail/NotAbigailPortrait.png"
}
```
Secondly, we need to load a spritesheet for the NPC.
```json
{
    "Action": "Load",
    "Target": "Characters/NotAbigail",
    "FromFile": "assets/NotAbigail/NotAbigail.png"
}
```

Thirdly, and optionally, we can add some dialogue if we want the NPC to speak. Most dialogue commands should work, but let me know if you run into anything that doesn't work as expected.

For dialogue keys, location-specific keys should work, but keep in mind that the NPC exists only where you put it. You could have the location of the NPC set via the tile property conditional, and have a different dialogue key for each location.
```json
{
    "Action": "EditData",
    "Target": "MEEP/FakeNPC/Dialogue/NotAbigail",
    "Entries": {
        "DialogueOne": "Hey @, you think you could build a raft?#$e#I saw a few cool islands on the way here I want to visit."
    }
}
```

Finally, we need to add a tile property to specify where we want the NPC to spawn.
```json
{
    "Action": "EditMap",
    "Target": "Maps/Town"
    "MapTiles": [
        {
            "Position": {
                "X": 29,
                "Y": 56
            },
            "Layer": "Back",
            "SetProperties": {
                "MEEP_FakeNPC": "NotAbigail"
            }
        }
    ]
}
```
