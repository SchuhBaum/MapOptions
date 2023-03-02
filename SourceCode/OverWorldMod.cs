namespace MapOptions;

internal static class OverWorldMod
{
    internal static void OnDisable()
    {
        On.OverWorld.LoadWorld -= OverWorld_LoadWorld;
        On.OverWorld.WorldLoaded -= OverWorld_WorldLoaded;
    }

    internal static void OnEnable() // Option_UncoverRegion
    {
        On.OverWorld.LoadWorld += OverWorld_LoadWorld; // load first region
        On.OverWorld.WorldLoaded += OverWorld_WorldLoaded; // change regions
    }

    //
    // public
    //

    public static void UncoverAllRooms(World world)
    {
        foreach (AbstractRoom abstractRoom in world.abstractRooms)
        {
            if (!MapMod.uncoveredRooms.Contains(abstractRoom))
            {
                MapMod.uncoveredRooms.Add(abstractRoom);
            }
        }
    }

    //
    // private
    //

    private static void OverWorld_LoadWorld(On.OverWorld.orig_LoadWorld orig, OverWorld overWorld, string worldName, SlugcatStats.Name slugcatName, bool singleRoomWorld)
    {
        orig(overWorld, worldName, slugcatName, singleRoomWorld);

        if (overWorld.game == null) return;
        if (!overWorld.game.IsStorySession) return;
        UncoverAllRooms(overWorld.activeWorld);
    }

    private static void OverWorld_WorldLoaded(On.OverWorld.orig_WorldLoaded orig, OverWorld overWorld)
    {
        orig(overWorld);

        if (overWorld.game == null) return;
        if (!overWorld.game.IsStorySession) return;
        UncoverAllRooms(overWorld.activeWorld);
    }
}