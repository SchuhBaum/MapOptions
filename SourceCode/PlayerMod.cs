using System.Collections.Generic;

using static MapOptions.MainMod;

namespace MapOptions;

public static class PlayerMod {
    internal static void On_Config_Changed() {
        On.Player.Regurgitate -= Player_Regurgitate;
        On.Player.SwallowObject -= Player_SwallowObject;

        if (Option_CreatureSymbols) {
            //On.Player.ctor += Player_ctor; // add or update slugcat symbol // maps not initialized yet => does nothing
            On.Player.Regurgitate += Player_Regurgitate; // add creature symbol if needed
            On.Player.SwallowObject += Player_SwallowObject; // remove creature symbol if needed
        }
    }

    //
    // public
    //

    public static void Remove_ObjectInStomach_Symbol(Player player) {
        if (player.objectInStomach is AbstractCreature abstract_creature) {
            foreach (MapMod.AttachedFields attached_fields in MapMod._all_attached_fields.Values) {
                foreach (Creature_Symbol_On_Map creature_symbol in attached_fields.creature_symbols) {
                    if (creature_symbol.abstract_creature == abstract_creature) {
                        creature_symbol.Remove_Sprites();
                        attached_fields.creature_symbols.Remove(creature_symbol);
                        break;
                    }
                }
            }
        }
    }

    //
    // private
    //

    private static void Player_Regurgitate(On.Player.orig_Regurgitate orig, Player player) {
        if (player.objectInStomach is AbstractCreature abstract_creature && !AbstractCreatureMod.creature_type_blacklist.Contains(abstract_creature.creatureTemplate.type)) {
            foreach (KeyValuePair<HUD.Map, MapMod.AttachedFields> map_attachedfields in MapMod._all_attached_fields) {
                map_attachedfields.Value.creature_symbols.Add(new Creature_Symbol_On_Map(abstract_creature, map_attachedfields.Key.inFrontContainer));
            }
        }
        orig(player);
    }

    private static void Player_SwallowObject(On.Player.orig_SwallowObject orig, Player player, int grasp) {
        orig(player, grasp);
        Remove_ObjectInStomach_Symbol(player);
    }
}
