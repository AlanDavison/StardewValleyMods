namespace DecidedlyShared.APIs;

public interface IItemExtensionsApi
{
    /// <summary>
    /// Gets breaking tool for a specific resource.
    /// </summary>
    /// <param name="id">The ID if the resource.</param>
    /// <param name="isClump">Whether it's a clump (instead of a node).</param>
    /// <param name="tool">The breaking tool.</param>
    /// <returns>Whether the resource data was found.</returns>
    bool GetBreakingTool(string id, bool isClump, out string tool);
}
