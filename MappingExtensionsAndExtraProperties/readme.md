# Mapping Extensions and Extra Properties (MEEP)

All releases can be found on my [Nexus page](https://www.nexusmods.com/users/79440738?tab=user+files).

## What it does
This mod does nothing on its own. Its primary purpose is to allow map authors to spice up their maps with the new custom tile properties, extra features, etc., that this mod adds.

## Current tile properties
| **Tile Property**          | **Layer** | **Description**                                                                                                                                                                                                                                     |
|----------------------------|-----------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| _CloseupInteraction_Image_ | Back      | This tile property will display a specified image on the screen when the player interacts with the tile it's placed on. If you want the player to be able to examine a photo on a desk and actually see the photo up-close, this is the one to use. |
| _CloseupInteraction_Text_  | Back      | This tile property only works in conjunction with `CloseupInteraction_Image`, and will display the specified text as a description below the image.                                                                                                 |


## Using the tile properties
Using the tile properties is fairly simple. There are a few things you'll need to know that I won't be covering here:
1) The basics of creating a Content Patcher pack. See [here](https://github.com/Pathoschild/StardewMods/blob/develop/ContentPatcher/docs/author-guide.md).
2) How to load an image asset using Content Patcher. See [here](https://github.com/Pathoschild/StardewMods/blob/develop/ContentPatcher/docs/author-guide/action-load.md).
3) How to patch tile properties using Content Patcher (see [here](https://github.com/Pathoschild/StardewMods/blob/develop/ContentPatcher/docs/author-guide/action-editmap.md#edit-map-tiles)), or how to add tile properties to your map directly using Tiled (see [here](https://stardewvalleywiki.com/Modding:Maps#Tile_properties)).

### Using the `CloseupInteraction` tile properties
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
                        "CloseupInteraction_Image": "Mods/DecidedlyHuman/PierreCounterThing",
                        "CloseupInteraction_Text": "I'm not even sure what this is... is it a plant pot, or a tomato?"
                    }
                }
            ]
        }
    ]
}
```
`CloseupInteraction_Image` takes a few "arguments" in its tile property value. The first, and only mandatory one, is the asset name (which can be a built-in Stardew image). The second is the region of the specified image you want to be displayed.


For example...
```json
"CloseupInteraction_Image": "LooseSprites/Cursors 540 305 42 28",
"CloseupInteraction_Text": "The spirits tell me you're learning how to use a new mod..."
```
Will display the fortune teller, and a message reading "The spirits tell me you're learning how to use a new mod...".

In `540 305 42 28`, `540` is the x co-ordinate of the top-left corner of the region of the specified image you want to display, `305` is the y co-ordinate, `42` is the width, and `28` is the height.

**Warning**: It's worth keeping in mind the size of the image, and whether or not it will interfere with Stardew Valley when running at lower resolutions when combined with the text display option. I recommend you **always test your images at a varying UI scale settings and window sizes** if you want to play it safe.
