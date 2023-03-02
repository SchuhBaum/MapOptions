using System.Collections.Generic;
using HUD;

using static MapOptions.MainMod;
using static MapOptions.MapMod;

namespace MapOptions;

public static class AbstractCreatureMod
{
    //
    // parameters
    //

    public static readonly List<CreatureTemplate.Type> creatureTypeBlacklist = new() { CreatureTemplate.Type.StandardGroundCreature, CreatureTemplate.Type.Fly, CreatureTemplate.Type.Slugcat };

    //
    // variables
    //

    private static bool is_enabled = false;

    //
    //
    //

    internal static void On_Toggle()
    {
        is_enabled = !is_enabled;
        if (Option_CreatureSymbols)
        {
            if (is_enabled)
            {
                // adds creature symbols 
                // map might not be initialized yet => add when maps get initialized
                On.AbstractCreature.ctor += AbstractCreature_ctor;
            }
            else
            {
                On.AbstractCreature.ctor -= AbstractCreature_ctor;
            }
        }
    }

    //
    // private
    //

    private static void AbstractCreature_ctor(On.AbstractCreature.orig_ctor orig, AbstractCreature abstractCreature, World world, CreatureTemplate creatureTemplate, Creature realizedCreature, WorldCoordinate pos, EntityID ID)
    {
        orig(abstractCreature, world, creatureTemplate, realizedCreature, pos, ID);
        if (!creatureTypeBlacklist.Contains(abstractCreature.creatureTemplate.type))
        {
            foreach (KeyValuePair<Map, AttachedFields> map_attachedFields in MapMod.all_attached_fields)
            {
                map_attachedFields.Value.creature_symbols.Add(new Creature_Symbol_On_Map(abstractCreature, map_attachedFields.Key.inFrontContainer));
            }
        }
    }
}