# Mapping Extensions and Extra Properties (MEEP) Documentation
**Important**: Remember to read the section on [adding the correct keys to your manifest](../readme.md#Adding-MEEP-feature-keys-to-your-manifest) for the specific features you intend to use first.

Note that some features (currently event commands) do *not* require any keys in your manifest.

### Closeup interaction trigger actions

**Important notes**:
* This feature requires SpaceCore. Remember to mark SpaceCore as a requirement in your mod if you use it. See the [wiki's manifest page](https://stardewvalleywiki.com/Modding:Modder_Guide/APIs/Manifest) for how to do so.
* When being used with items that can be eaten or otherwise users normally (food, books, etc.), this feature *will not work*.

This feature consists of a few parts.

1) An entry added to `spacechase0.SpaceCore/ObjectExtensionData` using Content Patcher's `EditData` action under the item ID you want the interaction on.
2) An entry added to `Data/TriggerActions` using ContentPatcher's `EditData` action.

See [Content Patcher's documentation](https://github.com/Pathoschild/StardewMods/blob/develop/ContentPatcher/docs/author-guide/action-editdata.md) for the specifics of how to do so.

For this example, we'll add an interaction to the Golden Animal Cracker. We want our `spacechase0.SpaceCore/ObjectExtensionData` entry to look as follows:

```json
{
    "Action": "EditData",
    "Target": "spacechase0.SpaceCore/ObjectExtensionData",
    "Entries": {
        "GoldenAnimalCracker": {
            "UseForTriggerAction": true
        }
    }
}
```

In this case, we're adding an entry named `GoldenAnimalCracker` to have this interaction occur when the player "uses" the Golden Animal Cracker. This tells SpaceCore to consider this item being used for its trigger action.

Next, we want to create our entry in `Data/TriggerActions`. This can look a little convoluted. This changes slightly depending on whether we're creating a `MEEP_CloseupInteraction_Action`, or a `MEEP_CloseupInteractionReel_Action`.

```json
{
    "Action": "EditData",
    "Target": "Data/TriggerActions",
    "Entries": {
        "DH.Test.GoldenAnimalCracker.CloseupInteraction.Trigger": {
            "Id": "DH.Test.GoldenAnimalCracker.CloseupInteraction.Trigger",
            "Trigger": "spacechase0.SpaceCore_OnItemUsed",
            "Condition": "ITEM_ID Input GoldenAnimalCracker",
            "Actions": [
                "MEEP_CloseupInteractionReel_Action"
            ],
            "CustomFields": {
                "MEEP_CloseupInteraction_Image_1": "Mods/DecidedlyHuman/PageOne",
                "MEEP_CloseupInteraction_Text_1": "Oh, wow. It's a golden animal cracker!",
                "MEEP_CloseupInteraction_Image_2": "Mods/DecidedlyHuman/PageTwo",
                "MEEP_CloseupInteraction_Text_2": "What does this say? I should feed it to one of my animals?",
                "MEEP_CloseupInteraction_Sound": "fishSlap"
            },
            "MarkActionApplied": false
        }
    }
}
```

Here, we're making an entry in `Data/TriggerActions` named `DH.Test.GoldenAnimalCracker.CloseupInteraction.Trigger`. This name is arbitrary, but it should be 100% unique. I would encourage something like `YourName.YourMod.WhatItsFor.Trigger`.

* `Id`: This needs to be the same as the name of the entry itself (in this case, `DH.Test.GoldenAnimalCracker.CloseupInteraction.Trigger`).
* `Trigger`: This needs to be `spacechase0.SpaceCore_OnItemUsed`.
* `Condition`: A [Game State Query](https://stardewvalleywiki.com/Modding:Game_state_queries) condition. If this evaluates to true, the trigger can occur.
* `Actions`: This needs to be a list with a single entry - `MEEP_CloseupInteractionReel_Action` (for a MEEP closeup interaction reel), or `MEEP_CloseupInteraction_Action` (for a single MEEP closeup interaction).
* `CustomFields`: This is where the arguments for the MEEP closeup interaction go. This follows the exact same syntax and style as [MEEP's normal closeup interactions](tile-properties.md#Using-the-CloseupInteraction-tile-properties).

With this done, the interaction should occur when a player "uses" the item.
