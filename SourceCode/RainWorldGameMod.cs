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
            MainModOptions.instance.MainModOptions_OnConfigChanged();

            if (MainMod.Option_CreatureSymbols)
            {
                AbstractCreatureMod.OnEnable();
                PlayerMod.OnEnable();
            }

            if (MainMod.Option_UncoverRegion)
            {
                OverWorldMod.OnEnable();
            }

            if (MainMod.Option_UncoverRoom)
            {
                AbstractRoomMod.OnEnable(); // when slugcat is added to an abstract room
                RegionGateMod.OnEnable(); // the room stays the same // wait a bit and add the room again to be uncovered in the new region as well
            }
            orig(game, manager);
        }

        private static void RainWorldGame_ShutDownProcess(On.RainWorldGame.orig_ShutDownProcess orig, RainWorldGame game)
        {
            Debug.Log("MapOptions: Remove option specific hooks.");
            orig(game);

            if (MainMod.Option_CreatureSymbols)
            {
                AbstractCreatureMod.OnDisable();
                PlayerMod.OnDisable();
            }

            if (MainMod.Option_UncoverRegion)
            {
                OverWorldMod.OnDisable();
            }

            if (MainMod.Option_UncoverRoom)
            {
                AbstractRoomMod.OnDisable();
                RegionGateMod.OnDisable();
            }
        }
    }
}