namespace MapOptions
{
    internal static class AbstractRoomMod
    {
        internal static void OnEnable()
        {
            On.AbstractRoom.AddEntity += AbstractRoom_AddEntity;
        }

        internal static void OnDisable()
        {
            On.AbstractRoom.AddEntity -= AbstractRoom_AddEntity;
        }

        // ----------------- //
        // private functions //
        // ----------------- //

        private static void AbstractRoom_AddEntity(On.AbstractRoom.orig_AddEntity orig, AbstractRoom abstractRoom, AbstractWorldEntity abstractWorldEntity)
        {
            orig(abstractRoom, abstractWorldEntity);
            if (abstractWorldEntity is not AbstractCreature abstractCreature) return;

            if (abstractCreature.creatureTemplate.type == CreatureTemplate.Type.Slugcat && !MapMod.uncoveredRooms.Contains(abstractRoom))
            {
                MapMod.uncoveredRooms.Add(abstractRoom);
            }
        }
    }
}