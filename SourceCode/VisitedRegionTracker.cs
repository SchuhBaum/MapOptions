
namespace MapOptions;

public static class VisitedRegionTracker {
    //
    // Fields

    public static string? loaded_campaign = null;
    public static List<string>? visited_region_names = null;

    //
    // Initialization

    internal static void On_Config_Changed() {
        On.Menu.CharacterSelectPage.Singal -= Menu_CharacterSelectPage_Singal; // Expedition only;
        On.Menu.SlugcatSelectMenu.StartGame -= Menu_SlugcatSelectMenu_StartGame;
        On.PlayerProgression.LoadMapTexture -= PlayerProgression_LoadMapTexture;
        On.ProcessManager.RequestMainProcessSwitch_ProcessID -= ProcessManager_RequestMainProcessSwitch;

        if (Option_ClearMapProgress) {
            On.Menu.CharacterSelectPage.Singal += Menu_CharacterSelectPage_Singal;
            On.Menu.SlugcatSelectMenu.StartGame += Menu_SlugcatSelectMenu_StartGame;
            On.PlayerProgression.LoadMapTexture += PlayerProgression_LoadMapTexture;
            On.ProcessManager.RequestMainProcessSwitch_ProcessID += ProcessManager_RequestMainProcessSwitch;
        }
    }

    //
    // Public Methods

    public static void CreateNewVisitedRegionTracker(SlugcatStats.Name slugcat_name) {
        CreateNewVisitedRegionTracker(slugcat_name.ToString());
    }

    public static void CreateNewVisitedRegionTracker(string campaign) {
        loaded_campaign = campaign;
        visited_region_names = [];
    }

    public static void UnloadVisitedRegionTracker() {
        loaded_campaign = null;
        visited_region_names = null;
    }

    //

    public static void LoadVisitedRegionNames(SlugcatStats.Name slugcat_name) {
        LoadVisitedRegionNames(slugcat_name.ToString());
    }

    public static void LoadVisitedRegionNames(string campaign) {
        string file_path = GetFilePath(campaign);
        if (!File.Exists(file_path)) {
            UnloadVisitedRegionTracker();
            return;
        }

        try {
            Debug.Log("MapOptions: Load visited region names from \"" + Path.GetFileName(file_path) + "\".");
            loaded_campaign = campaign;

            string json = File.ReadAllText(file_path);
            visited_region_names = json.listFromJson()
                .Select(obj => obj.ToString())
                .ToList();

        } catch (Exception ex) {
            Debug.Log($"{mod_id}: Loading failed.");
            Debug.Log($"{mod_id}: {ex}");
            UnloadVisitedRegionTracker();
        }
    }

    public static void SaveVisitedRegionNames() {
        if (loaded_campaign == null) return;
        if (visited_region_names == null) return;

        try {
            string file_path = GetFilePath(loaded_campaign);
            Debug.Log("MapOptions: Save visited region names to \"" + Path.GetFileName(file_path) + "\".");
            File.WriteAllText(file_path, Json.Serialize(visited_region_names));

        } catch (Exception ex) {
            Debug.Log($"{mod_id}: Saving failed.");
            Debug.Log($"{mod_id}: {ex}");
        }
    }

    //
    // Private Methods

    private static string GetFilePath(string campaign) {
        int save_slot = Custom.rainWorld.options.saveSlot;
        if (campaign.Equals("expedition", InvariantCultureIgnoreCase)) {
            return mod_directory_path + "region_names_of_cleared_maps" + save_slot + ".json";
        } else {
            return mod_directory_path + "visited_region_names_" + save_slot + "_" + campaign + ".json";
        }
    }

    //

    private static void Menu_CharacterSelectPage_Singal(On.Menu.CharacterSelectPage.orig_Singal orig, Menu.CharacterSelectPage character_select_page, Menu.MenuObject sender, string message) {
        orig(character_select_page, sender, message);

        if (sender == character_select_page.confirmExpedition) {
            if (message == "NEW") {
                // the variable is saved when entering the game by the process_manager;
                Debug.Log("MapOptions: Start new expedition.");
                CreateNewVisitedRegionTracker("expedition");
                return;
            }

            if (message == "LOAD") {
                Debug.Log("MapOptions: Load expedition.");
                LoadVisitedRegionNames("expedition");
                return;
            }
        }
    }

    private static void Menu_SlugcatSelectMenu_StartGame(On.Menu.SlugcatSelectMenu.orig_StartGame orig, SlugcatSelectMenu slugcat_select_menu, SlugcatStats.Name slugcat_name) {
        orig(slugcat_select_menu, slugcat_name);

        if (!slugcat_select_menu.restartChecked && slugcat_select_menu.manager.rainWorld.progression.IsThereASavedGame(slugcat_name)) {
            Debug.Log($"MapOptions: Load compaign for slugcat {slugcat_name}.");
            LoadVisitedRegionNames(slugcat_name);

        } else {
            Debug.Log($"MapOptions: Start new campaign for slugcat {slugcat_name}.");
            CreateNewVisitedRegionTracker(slugcat_name);
        }
    }

    private static void PlayerProgression_LoadMapTexture(On.PlayerProgression.orig_LoadMapTexture orig, PlayerProgression player_progression, string region_name) {
        // the function orig() might return early without loading the map; but the function
        // map.Update() calls this function only once; it only has once chance to load =>
        // reset map progress in any case;
        orig(player_progression, region_name);

        if (visited_region_names == null) return;
        if (visited_region_names.Contains(region_name)) return;
        Debug.Log("MapOptions: Clear map progress for region " + region_name + ".");

        // this would not work for visited rooms; the game recovers the progress in that 
        // case; it's okay here since for each new run there are no visited rooms;
        player_progression.mapDiscoveryTextures[region_name] = null;
        visited_region_names.Add(region_name);
    }

    private static void ProcessManager_RequestMainProcessSwitch(On.ProcessManager.orig_RequestMainProcessSwitch_ProcessID orig, ProcessManager process_manager, ProcessID next_process_id) {
        orig(process_manager, next_process_id);

        if (next_process_id == ProcessID.Game) {
            SaveVisitedRegionNames();
            return;
        }

        if (next_process_id == ProcessID.MainMenu) {
            SaveVisitedRegionNames();
            UnloadVisitedRegionTracker();
            return;
        }
    }
}
