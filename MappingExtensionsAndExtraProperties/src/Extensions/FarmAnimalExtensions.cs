using StardewValley;

namespace MappingExtensionsAndExtraProperties.Extensions;

public static class FarmAnimalExtensions
{
    public static bool IsMeepFarmAnimal(this FarmAnimal animal)
    {
        return animal.Name.StartsWith("DH.MEEP.SpawnedAnimal_");
    }
}
