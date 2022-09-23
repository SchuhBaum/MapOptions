using UnityEngine;

namespace MapOptions
{
    internal static class HUDMod
    {
        internal static void OnEnable()
        {
            On.HUD.HUD.ResetMap += HUD_ResetMap;
        }

        // ----------------- //
        // private functions //
        // ----------------- //

        private static void HUD_ResetMap(On.HUD.HUD.orig_ResetMap orig, HUD.HUD hud, HUD.Map.MapData mapData)
        {
            if (hud.map != null)
            {
                Debug.Log("MapOptions: Map is being deleted. Cleanup.");
                MapMod.Destroy(hud.map);
            }
            orig(hud, mapData);
        }
    }
}