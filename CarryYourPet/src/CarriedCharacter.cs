using StardewValley;
using StardewValley.Characters;

namespace CarryYourPet
{
    public class CarriedCharacter
    {
        private NPC npc;
        private bool shouldDraw;
        // private bool isPet;

        // public bool IsPet
        // {
        //     get => isPet;
        //     set
        //     {
        //         isPet = value;
        //     }
        // }
        
        public bool ShouldDraw
        {
            get => shouldDraw;
            set
            {
                shouldDraw = value;
            }
        }
        
        public NPC Npc
        {
            get => npc;
            set
            {
                npc = value;
            }
        }
    }
}