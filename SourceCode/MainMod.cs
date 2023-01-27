using System.Security;
using System.Security.Permissions;
using BepInEx;
using UnityEngine;

// temporary fix // should be added automatically //TODO
[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
namespace MapOptions
{
    [BepInPlugin("SchuhBaum.MapOptions", "MapOptions", "2.0.2")]
    public class MainMod : BaseUnityPlugin
    {
        //
        // meta data
        //

        public static readonly string MOD_ID = "MapOptions";
        public static readonly string author = "SchuhBaum";
        public static readonly string version = "2.0.2";

        //
        // options
        //

        public static bool Option_CreatureSymbols => MainModOptions.creatureSymbols.Value;
        public static bool Option_SlugcatSymbols => MainModOptions.slugcatSymbols.Value;
        public static bool Option_UncoverRegion => MainModOptions.uncoverRegion.Value;

        public static bool Option_UncoverRoom => MainModOptions.uncoverRoom.Value;
        public static bool Option_LayerFocus => MainModOptions.layerFocus.Value;
        public static bool Option_SkipFade => MainModOptions.skipFade.Value;

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

            if (isInitialized) return;
            isInitialized = true;

            Debug.Log("MapOptions: Version " + Info.Metadata.Version);
            MachineConnector.SetRegisteredOI(MOD_ID, MainModOptions.instance);

            if (!ModManager.JollyCoop)
            {
                Debug.Log("MapOptions: JollyCoop not found.");
            }
            else
            {
                Debug.Log("MapOptions: JollyCoop found. Use custom colors for slugcat symbols.");
            }

            MapMod.OnEnable();
            RainWorldGameMod.OnEnable();
        }
    }
}