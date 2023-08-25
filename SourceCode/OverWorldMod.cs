using static MapOptions.MainMod;

namespace MapOptions;

public static class OverWorldMod {
    internal static void On_Config_Changed() {
        On.OverWorld.LoadWorld -= OverWorld_LoadWorld;
        On.OverWorld.WorldLoaded -= OverWorld_WorldLoaded;

        if (Option_UncoverRegion) {
            On.OverWorld.LoadWorld += OverWorld_LoadWorld; // load first region
            On.OverWorld.WorldLoaded += OverWorld_WorldLoaded; // change regions
        }
    }

    //
    // public
    //

    public static void UncoverAllRooms(World world) {
        foreach (AbstractRoom abstract_room in world.abstractRooms) {
            if (!MapMod.uncovered_rooms.Contains(abstract_room)) {
                MapMod.uncovered_rooms.Add(abstract_room);
            }
        }
    }

    //
    // private
    //

    private static void OverWorld_LoadWorld(On.OverWorld.orig_LoadWorld orig, OverWorld over_world, string world_name, SlugcatStats.Name slugcat_name, bool single_room_world) {
        orig(over_world, world_name, slugcat_name, single_room_world);

        if (over_world.game == null) return;
        if (!over_world.game.IsStorySession) return;
        UncoverAllRooms(over_world.activeWorld);
    }

    private static void OverWorld_WorldLoaded(On.OverWorld.orig_WorldLoaded orig, OverWorld over_world) {
        orig(over_world);

        if (over_world.game == null) return;
        if (!over_world.game.IsStorySession) return;
        UncoverAllRooms(over_world.activeWorld);
    }
}
