# Buff Crops

All releases can be found on my [Nexus page](https://www.nexusmods.com/users/79440738?tab=user+files), and (if I
remember to auto-sync them), my [ModDrop](https://www.moddrop.com/stardew-valley/profile/251772/mods) page.

### What it does
This mod gives you various buffs depending on the crops you have planted and fully grown on your farm. Giant crops are more potent, giving you more reason than just looks to keep them around.

I highly recommend pairing this with 6480's Giant Crops. This mod comes with buff values for those new giant crops pre-loaded.

This is the first version, so balance might be a little wonky. **Please let me know if you have any suggestions!**

### Extra information
Mod authors can easily add buffs to their own crops like so:

﻿```
"Changes": [
    {
        "Action": "EditData",
        "Target": "Data/Crops",
        "Fields": {
            // SPRING
            "476": { // Garlic
                "CustomFields": {
                    "DH.BuffCrops.BuffContribution": "Speed 0.01 CombatLevel 0.01"
                }
            },
        }
    }
]
```

> Valid buffs:
> CombatLevel
> FarmingLevel
> FishingLevel
> MiningLevel
> LuckLevel
> ForagingLevel
> MaxStamina
> MagneticRadius
> Speed
> Defense
> Attack
> Immunity
> AttackMultiplier
> KnockbackMultiplier
> WeaponSpeedMultiplier
> CriticalChanceMultiplier
> CriticalPowerMultiplier
> WeaponPrecisionMultiplier
