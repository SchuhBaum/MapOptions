using System.Security.Permissions;
using BepInEx;
using MonoMod.Cil;
using UnityEngine;

using static MapOptions.MainModOptions;

// temporary fix // should be added automatically //TODO
#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
namespace MapOptions;

[BepInPlugin("SchuhBaum.MapOptions", "MapOptions", "2.0.9")]
public class MainMod : BaseUnityPlugin
{
    //
    // meta data
    //

    public static readonly string MOD_ID = "MapOptions";
    public static readonly string author = "SchuhBaum";
    public static readonly string version = "2.0.9";

    //
    // options
    //

    public static bool Option_AerialMap => aerial_map.Value;
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

    public static bool is_initialized = false;

    //
    // main
    //

    public MainMod() { }
    public void OnEnable() => On.RainWorld.OnModsInit += RainWorld_OnModsInit; // look for dependencies and initialize hooks

    //
    // public
    //

    public static void LogAllInstructions(ILContext? context, int indexStringLength = 9, int opCodeStringLength = 14)
    {
        if (context == null) return;

        Debug.Log("-----------------------------------------------------------------");
        Debug.Log("Log all IL-instructions.");
        Debug.Log("Index:" + new string(' ', indexStringLength - 6) + "OpCode:" + new string(' ', opCodeStringLength - 7) + "Operand:");

        ILCursor cursor = new(context);
        ILCursor labelCursor = cursor.Clone();

        string cursorIndexString;
        string opCodeString;
        string operandString;

        while (true)
        {
            // this might return too early;
            // if (cursor.Next.MatchRet()) break;

            // should always break at some point;
            // only TryGotoNext() doesn't seem to be enough;
            // it still throws an exception;
            try
            {
                if (cursor.TryGotoNext(MoveType.Before))
                {
                    cursorIndexString = cursor.Index.ToString();
                    cursorIndexString = cursorIndexString.Length < indexStringLength ? cursorIndexString + new string(' ', indexStringLength - cursorIndexString.Length) : cursorIndexString;
                    opCodeString = cursor.Next.OpCode.ToString();

                    if (cursor.Next.Operand is ILLabel label)
                    {
                        labelCursor.GotoLabel(label);
                        operandString = "Label >>> " + labelCursor.Index;
                    }
                    else
                    {
                        operandString = cursor.Next.Operand?.ToString() ?? "";
                    }

                    if (operandString == "")
                    {
                        Debug.Log(cursorIndexString + opCodeString);
                    }
                    else
                    {
                        opCodeString = opCodeString.Length < opCodeStringLength ? opCodeString + new string(' ', opCodeStringLength - opCodeString.Length) : opCodeString;
                        Debug.Log(cursorIndexString + opCodeString + operandString);
                    }
                }
                else
                {
                    break;
                }
            }
            catch
            {
                break;
            }
        }
        Debug.Log("-----------------------------------------------------------------");
    }

    //
    // private
    //

    private void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld rainWorld)
    {
        orig(rainWorld);
        MachineConnector.SetRegisteredOI(MOD_ID, instance);

        if (is_initialized) return;
        is_initialized = true;

        Debug.Log("MapOptions: version " + version);
        MapMod.OnEnable();
        RainWorldGameMod.OnEnable();
    }
}