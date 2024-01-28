using BepInEx;
using MonoMod.Cil;
using System.IO;
using System.Reflection;
using System.Security.Permissions;
using UnityEngine;
using static MapOptions.MainModOptions;

// allows access to private members;
#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618

namespace MapOptions;

[BepInPlugin("SchuhBaum.MapOptions", "MapOptions", "2.1.6")]
public class MainMod : BaseUnityPlugin {
    //
    // meta data
    //

    public static readonly string mod_id = "MapOptions";
    public static readonly string author = "SchuhBaum";
    public static readonly string version = "2.1.6";
    public static readonly string mod_directory_path = Directory.GetParent(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).FullName + Path.DirectorySeparatorChar;

    //
    // options
    //

    public static bool Option_AerialMap => aerial_map.Value;
    public static bool Option_ClearExpeditionMaps => clear_expedition_maps.Value;
    public static bool Option_CreatureSymbols => creature_symbols.Value;
    public static bool Option_ItemTracker => item_tracker.Value;

    public static bool Option_LayerFocus => layer_focus.Value;
    public static bool Option_ShadowSprites => shadow_sprites.Value;
    public static bool Option_SkipFade => skip_fade.Value;
    public static bool Option_SlugcatSymbols => slugcat_symbols.Value;

    public static bool Option_UncoverRegion => uncover_region.Value;
    public static bool Option_UncoverRoom => uncover_room.Value;

    //
    // variables
    //

    public static bool can_log_il_hooks = false;
    private static bool _is_initialized = false;

    //
    // main
    //

    public MainMod() { }
    public void OnEnable() => On.RainWorld.OnModsInit += RainWorld_OnModsInit; // look for dependencies and initialize hooks

    //
    // public
    //

    public static void LogAllInstructions(ILContext? context, int index_string_length = 9, int op_code_string_length = 14) {
        if (context == null) return;

        Debug.Log("-----------------------------------------------------------------");
        Debug.Log("Log all IL-instructions.");
        Debug.Log("Index:" + new string(' ', index_string_length - 6) + "OpCode:" + new string(' ', op_code_string_length - 7) + "Operand:");

        ILCursor cursor = new(context);
        ILCursor label_cursor = cursor.Clone();

        string cursor_index_string;
        string op_code_string;
        string operand_string;

        while (true) {
            // this might return too early;
            // if (cursor.Next.MatchRet()) break;

            // should always break at some point;
            // only TryGotoNext() doesn't seem to be enough;
            // it still throws an exception;
            try {
                if (cursor.TryGotoNext(MoveType.Before)) {
                    cursor_index_string = cursor.Index.ToString();
                    cursor_index_string = cursor_index_string.Length < index_string_length ? cursor_index_string + new string(' ', index_string_length - cursor_index_string.Length) : cursor_index_string;
                    op_code_string = cursor.Next.OpCode.ToString();

                    if (cursor.Next.Operand is ILLabel label) {
                        label_cursor.GotoLabel(label);
                        operand_string = "Label >>> " + label_cursor.Index;
                    } else {
                        operand_string = cursor.Next.Operand?.ToString() ?? "";
                    }

                    if (operand_string == "") {
                        Debug.Log(cursor_index_string + op_code_string);
                    } else {
                        op_code_string = op_code_string.Length < op_code_string_length ? op_code_string + new string(' ', op_code_string_length - op_code_string.Length) : op_code_string;
                        Debug.Log(cursor_index_string + op_code_string + operand_string);
                    }
                } else {
                    break;
                }
            } catch {
                break;
            }
        }
        Debug.Log("-----------------------------------------------------------------");
    }

    //
    // private
    //

    private void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld rain_world) {
        orig(rain_world);
        MachineConnector.SetRegisteredOI(mod_id, main_mod_options);

        if (_is_initialized) return;
        _is_initialized = true;

        Debug.Log(mod_id + ": version " + version);
        MapMod.OnEnable();
        ProcessManagerMod.OnEnable();
    }
}
