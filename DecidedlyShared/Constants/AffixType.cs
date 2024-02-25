namespace DecidedlyShared.Constants
{
    /// <summary>
    /// Indicates the type of affix for a Harmony patch. Can be either a prefix, or a postfix.
    /// </summary>
    public enum AffixType
    {
        /// <summary>
        /// A prefix. This has the potential to override a method entirely.
        /// </summary>
        Prefix,

        /// <summary>
        /// A postfix. This runs at the end of a patched method.
        /// </summary>
        Postfix
    }
}
