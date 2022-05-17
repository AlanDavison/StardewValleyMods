using StardewValley;

namespace SmartBuilding
{
    /// <summary>
    /// Basic Item metadata.
    /// </summary>
    struct ItemInfo
    {
        /// <summary>
        /// A reference to the item to be placed.
        /// </summary>
        public Item Item;

        /// <summary>
        /// The basic type of item that it is, determined by <see cref="ModEntry.IdentifyItemType"/>
        /// </summary>
        public ItemType ItemType;

        /// <summary>
        /// Whether this item is destined to be inserted into a machine.
        /// </summary>
        public bool ToBeInserted;
    }
}