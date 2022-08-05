namespace SmartBuilding
{
    public enum ProducerType
    {
        /// <summary>
        ///     This means the game automatically removes recipe items from the player's inventory (i.e., furnaces).
        /// </summary>
        AutomaticRemoval,

        /// <summary>
        ///     This means we need to handle the removal of items ourselves.
        /// </summary>
        ManualRemoval,

        /// <summary>
        ///     This is purely for Prismatic Fire integration.
        /// </summary>
        TechnicallyNotAProducerButIsATorch,

        /// <summary>
        ///     This is not a producer.
        /// </summary>
        NotAProducer
    }
}
