using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace EveryoneIsGrandpa.src
{
    public class ModEntry : Mod
    {
        private Texture2D grandpa;
        private IModHelper helper;

        public override void Entry(IModHelper helper)
        {
            this.helper = helper;
            helper.Events.Content.AssetRequested += this.Content_AssetRequested;
            //grandpa = helper.Content.Load<Texture2D>("Characters/DecidedlyHumansCurse/Grandpa");

            Patches.Init();

            var harmony = new Harmony(this.ModManifest.UniqueID);

            harmony.Patch(
                AccessTools.Method(typeof(NPC), nameof(NPC.draw), new[] { typeof(SpriteBatch), typeof(float) }),
                new HarmonyMethod(typeof(Patches), nameof(Patches.NPC_draw_prefix)));
        }

        private void Content_AssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (e.Name.IsEquivalentTo("Characters/DecidedlyHumansCurse/Grandpa"))
                e.LoadFromModFile<Texture2D>("assets/Grandpa.png", AssetLoadPriority.Exclusive);
        }
    }
}
