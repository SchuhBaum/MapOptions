using UnityEngine;

namespace MapOptions;

internal static class RainWorldGameMod
{
    internal static void OnEnable()
    {
        On.RainWorldGame.ctor += RainWorldGame_ctor;
        On.RainWorldGame.ShutDownProcess += RainWorldGame_ShutDownProcess;
    }

    //
    // private
    //

    private static void RainWorldGame_ctor(On.RainWorldGame.orig_ctor orig, RainWorldGame game, ProcessManager manager)
    {
        Debug.Log("MapOptions: Add option specific hooks.");

        AbstractCreatureMod.On_Toggle();
        AbstractPhysicalObjectMod.OnToggle();
        AbstractRoomMod.On_Toggle();
        MapMod.OnToggle();

        OverWorldMod.On_Toggle();
        PlayerMod.On_Toggle();
        RegionGateMod.On_Toggle();

        orig(game, manager);
    }

    private static void RainWorldGame_ShutDownProcess(On.RainWorldGame.orig_ShutDownProcess orig, RainWorldGame game)
    {
        // I probably don't want to clear variables here since
        // the death and sleep screens have maps as well; 
        Debug.Log("MapOptions: Remove option specific hooks.");
        orig(game);

        AbstractCreatureMod.On_Toggle();
        AbstractPhysicalObjectMod.OnToggle();
        AbstractRoomMod.On_Toggle();
        MapMod.OnToggle();

        OverWorldMod.On_Toggle();
        PlayerMod.On_Toggle();
        RegionGateMod.On_Toggle();
    }
}