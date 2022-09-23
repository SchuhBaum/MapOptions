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
            if (hud.map?.GetAttachedFields() is MapMod.AttachedFields attachedFields)
            {
                MapMod.ClearAttachedFields(attachedFields);
            }
            orig(hud, mapData);
        }
    }
}