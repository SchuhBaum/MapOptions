using static MapOptions.MainMod;

namespace MapOptions;

internal static class AbstractRoomMod
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
                On.AbstractRoom.AddEntity += AbstractRoom_AddEntity;
            }
            else
            {
                On.AbstractRoom.AddEntity -= AbstractRoom_AddEntity;
            }
        }
    }

    //
    // private
    //

    private static void AbstractRoom_AddEntity(On.AbstractRoom.orig_AddEntity orig, AbstractRoom abstractRoom, AbstractWorldEntity abstractWorldEntity)
    {
        orig(abstractRoom, abstractWorldEntity);

        if (abstractWorldEntity is not AbstractCreature abstractCreature) return;
        if (abstractCreature.creatureTemplate.type != CreatureTemplate.Type.Slugcat) return;
        if (MapMod.uncovered_rooms.Contains(abstractRoom)) return;
        MapMod.uncovered_rooms.Add(abstractRoom);
    }
}