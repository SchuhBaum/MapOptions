namespace MapOptions
{
    internal static class WorldLoaderMod
    {
        internal static void OnDisable()
        {
            On.WorldLoader.NextActivity -= WorldLoader_NextActivity;
        }

        internal static void OnEnable()
        {
            On.WorldLoader.NextActivity += WorldLoader_NextActivity;
        }

        // ----------------- //
        // private functions //
        // ----------------- //

        private static void WorldLoader_NextActivity(On.WorldLoader.orig_NextActivity orig, WorldLoader worldLoader)
        {
            orig(worldLoader);

            if (!worldLoader.Finished) return;
            if (worldLoader.game?.IsStorySession == false) return;

            foreach (AbstractRoom abstractRoom in worldLoader.world.abstractRooms)
            {
                if (!MapMod.uncoveredRooms.Contains(abstractRoom))
                {
                    MapMod.uncoveredRooms.Add(abstractRoom);
                }
            }
        }
    }
}