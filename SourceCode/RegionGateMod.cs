using static MapOptions.MainMod;

namespace MapOptions;

internal static class RegionGateMod
{
    //
    // variables
    //

    private static bool is_enabled = false;

    //
    //
    //

    internal static void On_Toggle()
    {
        is_enabled = !is_enabled;
        if (Option_UncoverRoom)
        {
            if (is_enabled)
            {
                // the room stays the same 
                // wait a bit and add the room again to be uncovered in the new region as well
                On.RegionGate.Update += RegionGate_Update;
            }
            else
            {
                On.RegionGate.Update -= RegionGate_Update;
            }
        }
    }

    //
    // private
    //

    private static void RegionGate_Update(On.RegionGate.orig_Update orig, RegionGate regionGate, bool eu)
    {
        orig(regionGate, eu);

        // spams UncoverRoom() // its fine // once the pixels are uncovered the function just returns anyway
        if (regionGate.mode == RegionGate.Mode.Waiting && !regionGate.waitingForWorldLoader && !MapMod.uncoveredRooms.Contains(regionGate.room.abstractRoom))
        {
            MapMod.uncoveredRooms.Add(regionGate.room.abstractRoom);
        }
    }
}