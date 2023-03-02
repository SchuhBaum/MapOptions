namespace MapOptions;

internal static class RegionGateMod
{
    internal static void OnDisable()
    {
        On.RegionGate.Update -= RegionGate_Update;
    }

    internal static void OnEnable()
    {
        On.RegionGate.Update += RegionGate_Update;
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