using HUD;
using System.Collections.Generic;
using static MapOptions.MainMod;
using static MapOptions.MapMod;

namespace MapOptions;

public static class AbstractCreatureMod {
    //
    // parameters
    //

    public static readonly List<CreatureTemplate.Type> creature_type_blacklist = new() { CreatureTemplate.Type.StandardGroundCreature, CreatureTemplate.Type.Fly, CreatureTemplate.Type.Slugcat };

    //
    //
    //

    internal static void On_Config_Changed() {
        On.AbstractCreature.ctor -= AbstractCreature_Ctor;
        if (Option_CreatureSymbols) {
            // adds creature symbols 
            // map might not be initialized yet => add when maps get initialized
            On.AbstractCreature.ctor += AbstractCreature_Ctor;
        }
    }

    //
    // private
    //

    private static void AbstractCreature_Ctor(On.AbstractCreature.orig_ctor orig, AbstractCreature abstract_creature, World world, CreatureTemplate creature_template, Creature realized_creature, WorldCoordinate pos, EntityID id) {
        orig(abstract_creature, world, creature_template, realized_creature, pos, id);
        if (creature_type_blacklist.Contains(abstract_creature.creatureTemplate.type)) return;

        foreach (KeyValuePair<Map, AttachedFields> map_attached_fields in _all_attached_fields) {
            map_attached_fields.Value.creature_symbols.Add(new Creature_Symbol_On_Map(abstract_creature, map_attached_fields.Key.inFrontContainer));
        }
    }
}
