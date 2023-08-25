using static MapOptions.MainMod;

namespace MapOptions;

internal static class RegionGateMod {
    internal static void On_Config_Changed() {
        On.RegionGate.Update -= RegionGate_Update;
        if (Option_UncoverRoom) {
            // the room stays the same 
            // wait a bit and add the room again to be uncovered in the new region as well
            On.RegionGate.Update += RegionGate_Update;
        }
    }

    //
    // private
    //

    private static void RegionGate_Update(On.RegionGate.orig_Update orig, RegionGate region_gate, bool eu) {
        orig(region_gate, eu);

        // spams UncoverRoom() // its fine // once the pixels are uncovered the function just returns anyway
        if (region_gate.mode == RegionGate.Mode.Waiting && !region_gate.waitingForWorldLoader && !MapMod.uncovered_rooms.Contains(region_gate.room.abstractRoom)) {
            MapMod.uncovered_rooms.Add(region_gate.room.abstractRoom);
        }
    }
}
