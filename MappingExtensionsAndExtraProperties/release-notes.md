# MEEP Release Notes
* 2.4.6
  * Fix: Huge oversight in MEEP animal removal command that, if left enabled when auto-petting happened, would remove any auto-pet farm animal while the command is enabled.
* 2.4.5
  * Finally fully fix null reference oversight in farm animal spawns feature.
* 2.4.4
  * Fix null reference oversight in farm animal spawns feature.
* 2.4.3
  * Fix a mistake preventing auto petters from working.
* 2.4.2
  * Fix multiplayer farm animal dialogue issue.
* 2.4.0
  * Now you can make your custom wild or fruit trees invulnerable, meaning the player can't chop them down or blow them up.
  * Added portrait support to MEEP-spawned farm animals. This also works with portrait commands to display the happy, sad, etc. portraits.
  * Added an event command, allowing you to spawn slimes of specific colours during your events.
* 2.3.7
  * Fixed MEEP farm animals appearing in events.
  * Potentially fixed Lazy Mod spam-petting MEEP farm animals.
* 2.3.6
  * Fixed the animal-removal command. Oops!
* 2.3.5
  * Correctly neutered farm animals (I think), and added a command to allow for sending farm animals to another dimension (read: remove them permanently).
* 2.3.4
  * Fixed a problem with CJB's cheat menu constantly trying to pet our farm animals.
* 2.3.3
  * Fixed a problem with spawned farm animals appearing in 1.6's new animals tab.
* 2.3.2
  * Fixed a massive oversight with spawning invalid farm animals resulting in no later animals being spawned.
* 2.3.0
  * Full 1.6 release.
* 2.2.5
  * Fixed a bug with farm animal spawning.
* 2.2.2
  * "Fixed" (added because I forgot originally... oops) MEEP-spawned farm animals not being able to have their age specified.
* 2.2.0
  * Added a new tile property that allows you to set a custom conversation topic.
* 2.1.0-beta
  * Added the ability to spawn farm animals!
* 2.0.4-beta
  * Added a HUD message to make it clear you can't gift fake NPCs.
* 2.0.3-beta
  * Fixed a problem with the letter tile property.
* 2.0.2-beta
  * Fixed SpaceCore error.
* 2.0.0-beta
  * Release for 1.6 compatibility.
* 1.3.2
  * I did an oops. The UI close buttons work again, now!
* 1.3.1
  * Fixed Save Anywhere compatibility.
* 1.3.0
  * `MEEP_Letter_Type` tile property updated to allow for custom letter backgrounds.
* 1.2.0
  * `FakeNPC` tile property updated to add support for custom sprite sizes.
  * `MEEP_CloseupInteraction_Sound` tile property added, to allow the selection of a sound cue from the game to be played when the interaction is opened, and when pages in the reel are turned.
* 1.1.1
  * `Letter` tile properties added to allow map authors to trigger a custom letter/mail to appear when the player interacts with a given tile.
* 1.0.6
  * `FakeNPC` tile property added, allowing map authors to add NPCs that appear to be a real NPC, but without all of the setup required for a typical NPC.
  * `CloseupInteraction_Image` properties tweaked to allow specifying multiple images/descriptions that can be flipped through.
* 1.0.0
  * The mod exists.
  * `CloseupInteraction_Image`, `CloseupInteraction_Text`, and `DHSetMailFlag` tile properties added.
* 1.0.2
  * Added the MEEP_FakeNPC tile property. Renamed previous tile properties to match new MEEP_* naming scheme.
