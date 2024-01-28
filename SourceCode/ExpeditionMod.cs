using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static ProcessManager;
using static MapOptions.MainMod;
using RWCustom;

namespace MapOptions;

public static class ExpeditionMod {
    //
    // variables
    //

    public static List<string>? region_names_of_cleared_maps = null;

    //
    //
    //

    internal static void On_Config_Changed() {
        On.Menu.CharacterSelectPage.Singal -= Menu_CharacterSelectPage_Singal;
        On.PlayerProgression.LoadMapTexture -= PlayerProgression_LoadMapTexture;
        On.ProcessManager.RequestMainProcessSwitch_ProcessID -= ProcessManager_RequestMainProcessSwitch;

        if (Option_ClearExpeditionMaps) {
            On.Menu.CharacterSelectPage.Singal += Menu_CharacterSelectPage_Singal;
            On.PlayerProgression.LoadMapTexture += PlayerProgression_LoadMapTexture;
            On.ProcessManager.RequestMainProcessSwitch_ProcessID += ProcessManager_RequestMainProcessSwitch;
        }
    }

    //
    // public
    //

    public static void Load_Region_Names_Of_Cleared_Maps() {
        string file_path = mod_directory_path + "region_names_of_cleared_maps" + Custom.rainWorld.options.saveSlot + ".json";
        if (!File.Exists(file_path)) {
            region_names_of_cleared_maps = null;
            return;
        }

        try {
            Debug.Log("MapOptions: Load the variable region_names_of_cleared_maps for save slot " + Custom.rainWorld.options.saveSlot + ".");
            List<object> file_content = (List<object>)Json.Deserialize(File.ReadAllText(file_path));
            region_names_of_cleared_maps = new();

            foreach (object obj in file_content) {
                region_names_of_cleared_maps.Add(obj.ToString());
            }
        } catch { }
        return;
    }

    public static void Save_Region_Names_Of_Cleared_Maps() {
        if (region_names_of_cleared_maps == null) return;
        try {
            Debug.Log("MapOptions: Save the variable region_names_of_cleared_maps for save slot " + Custom.rainWorld.options.saveSlot + ".");
            string file_path = mod_directory_path + "region_names_of_cleared_maps" + Custom.rainWorld.options.saveSlot + ".json";
            File.WriteAllText(file_path, Json.Serialize(region_names_of_cleared_maps));
        } catch { }
    }

    //
    // private
    //

    private static void Menu_CharacterSelectPage_Singal(On.Menu.CharacterSelectPage.orig_Singal orig, Menu.CharacterSelectPage character_select_page, Menu.MenuObject sender, string message) {
        orig(character_select_page, sender, message);
        if (sender != character_select_page.confirmExpedition) return;

        if (message == "NEW") {
            // the variable is saved when entering the game by the process_manager;
            Debug.Log("MapOptions: Start new expedition.");
            region_names_of_cleared_maps = new();
            return;
        }

        if (message == "LOAD") {
            Debug.Log("MapOptions: Load expedition.");
            Load_Region_Names_Of_Cleared_Maps();
            return;
        }
    }

    private static void PlayerProgression_LoadMapTexture(On.PlayerProgression.orig_LoadMapTexture orig, PlayerProgression player_progression, string region_name) {
        // the function orig() might return early without loading the map; but the function
        // map.Update() calls this function only once; it only has once chance to load =>
        // reset map progress in any case;
        orig(player_progression, region_name);
        if (region_names_of_cleared_maps == null) return;
        if (region_names_of_cleared_maps.Contains(region_name)) return;
        Debug.Log("MapOptions: Clear expedition map progress for region " + region_name + ".");

        // this would not work for visited rooms; the game recovers the progress in that 
        // case; it's okay here since for each new run there are no visited rooms;
        player_progression.mapDiscoveryTextures[region_name] = null;
        region_names_of_cleared_maps.Add(region_name);
    }

    private static void ProcessManager_RequestMainProcessSwitch(On.ProcessManager.orig_RequestMainProcessSwitch_ProcessID orig, ProcessManager process_manager, ProcessID next_process_id) {
        orig(process_manager, next_process_id);
        if (next_process_id == ProcessID.Game) {
            Save_Region_Names_Of_Cleared_Maps();
            return;
        }

        if (next_process_id == ProcessID.MainMenu) {
            Save_Region_Names_Of_Cleared_Maps();
            region_names_of_cleared_maps = null;
            return;
        }
    }
}
