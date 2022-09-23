using UnityEngine;

namespace MapOptions
{
    internal static class RainWorldGameMod
    {
        internal static void OnEnable()
        {
            On.RainWorldGame.ctor += RainWorldGame_ctor;
            On.RainWorldGame.ShutDownProcess += RainWorldGame_ShutDownProcess;
        }

        // ----------------- //
        // private functions //
        // ----------------- //

        private static void RainWorldGame_ctor(On.RainWorldGame.orig_ctor orig, RainWorldGame game, ProcessManager manager)
        {
            Debug.Log("MapOptions: Add option specific hooks.");

            if (MainMod.creatureSymbolsOption)
            {
                AbstractCreatureMod.OnEnable();
                PlayerMod.OnEnable();
            }

            if (MainMod.uncoverRegionOption)
            {
                WorldLoaderMod.OnEnable();
            }

            if (MainMod.uncoverRoomOption)
            {
                AbstractRoomMod.OnEnable(); // when slugcat is added to an abstract room
                RegionGateMod.OnEnable(); // the room stays the same // wait a bit and add the room again to be uncovered in the new region as well
            }
            orig(game, manager);
        }

        private static void RainWorldGame_ShutDownProcess(On.RainWorldGame.orig_ShutDownProcess orig, RainWorldGame game)
        {
            Debug.Log("MapOptions: Cleanup. Remove option specific hooks.");
            orig(game);

            foreach (MapMod.AttachedFields attachedFields in MapMod.allAttachedFields.Values)
            {
                MapMod.ClearAttachedFields(attachedFields);
            }
            MapMod.allAttachedFields.Clear();

            if (MainMod.creatureSymbolsOption)
            {
                AbstractCreatureMod.OnDisable();
                PlayerMod.OnDisable();
            }

            if (MainMod.uncoverRegionOption)
            {
                WorldLoaderMod.OnDisable();
            }

            if (MainMod.uncoverRoomOption)
            {
                AbstractRoomMod.OnDisable();
                RegionGateMod.OnDisable();
            }
        }
    }
}