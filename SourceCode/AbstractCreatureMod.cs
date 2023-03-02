using System.Collections.Generic;

using static MapOptions.MainMod;

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
            foreach (KeyValuePair<HUD.Map, MapMod.AttachedFields> map_attachedFields in MapMod.allAttachedFields)
            {
                map_attachedFields.Value.creatureSymbols.Add(new Creature_Symbol_On_Map(abstractCreature, map_attachedFields.Key.inFrontContainer));
            }
        }
    }
}