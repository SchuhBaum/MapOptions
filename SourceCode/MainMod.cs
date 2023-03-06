using System.Security.Permissions;
using BepInEx;
using UnityEngine;

using static MapOptions.MainModOptions;

// temporary fix // should be added automatically //TODO
#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
namespace MapOptions;

[BepInPlugin("SchuhBaum.MapOptions", "MapOptions", "2.0.7")]
public class MainMod : BaseUnityPlugin
{
    //
    // meta data
    //

    public static readonly string MOD_ID = "MapOptions";
    public static readonly string author = "SchuhBaum";
    public static readonly string version = "2.0.7";

    //
    // options
    //

    public static bool Option_AerialMap => aerialMap.Value;
    public static bool Option_CreatureSymbols => creatureSymbols.Value;
    public static bool Option_LayerFocus => layerFocus.Value;
    public static bool Option_ShadowSprites => shadow_sprites.Value;

    public static bool Option_SkipFade => skipFade.Value;
    public static bool Option_SlugcatSymbols => slugcatSymbols.Value;
    public static bool Option_UncoverRegion => uncoverRegion.Value;
    public static bool Option_UncoverRoom => uncoverRoom.Value;

    //
    // variables
    //

    public static bool isInitialized = false;

    //
    // main
    //

    public MainMod() { }
    public void OnEnable() => On.RainWorld.OnModsInit += RainWorld_OnModsInit; // look for dependencies and initialize hooks

    //
    // private
    //

    private void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld rainWorld)
    {
        orig(rainWorld);
        MachineConnector.SetRegisteredOI(MOD_ID, instance);

        if (isInitialized) return;
        isInitialized = true;

        Debug.Log("MapOptions: version " + version);
        MapMod.OnEnable();
        RainWorldGameMod.OnEnable();
    }
}