using StardewValley;

namespace MappingExtensionsAndExtraProperties.Api;

public class MeepApi
{
    public bool IsFakeNpc(NPC npc) => npc is FakeNpc;
}
