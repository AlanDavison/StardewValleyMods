using Microsoft.Xna.Framework;

namespace MappingExtensionsAndExtraProperties.Models.FarmAnimals;

public class Animal
{
    public string? Id { get; set; }
    public string? AnimalId { get; set; }
    public string? SkinId { get; set; }
    public string? LocationId { get; set; }
    public string? DisplayName { get; set; }
    public string[]? PetMessage { get; set; }
    public int HomeTileX { get; set; }
    public int HomeTileY { get; set; }
    public string? Condition { get; set; }
}
