using static MapOptions.MainMod;

namespace MapOptions;

internal static class AbstractRoomMod {
    internal static void On_Config_Changed() {
        On.AbstractRoom.AddEntity -= AbstractRoom_AddEntity;
        if (Option_UncoverRoom) {
            On.AbstractRoom.AddEntity += AbstractRoom_AddEntity;
        }
    }

    //
    // private
    //

    private static void AbstractRoom_AddEntity(On.AbstractRoom.orig_AddEntity orig, AbstractRoom abstract_room, AbstractWorldEntity abstract_world_entity) {
        orig(abstract_room, abstract_world_entity);

        if (abstract_world_entity is not AbstractCreature abstract_creature) return;
        if (abstract_creature.creatureTemplate.type != CreatureTemplate.Type.Slugcat) return;
        if (MapMod.uncovered_rooms.Contains(abstract_room)) return;
        MapMod.uncovered_rooms.Add(abstract_room);
    }
}
