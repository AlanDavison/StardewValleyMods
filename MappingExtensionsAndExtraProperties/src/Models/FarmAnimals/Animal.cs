using xTile.Tiles;

namespace MappingExtensionsAndExtraProperties.Models.FarmAnimals;

public class Animal
{
    /// <summary>
    /// <see cref="Animal.Id"/> will have "_1", "_2", etc. appended for each one spawned.
    /// </summary>
    public string Id { get; set; }
    public string SkinId { get; set; }
    public string LocationId { get; set; }
    public string DisplayName { get; set; }
    public string PetMessage { get; set; }
    public Tile HomeTile { get; set; }
    public string Condition { get; set; }
}
