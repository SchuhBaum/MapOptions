using System.Collections.Generic;

using static MapOptions.MainMod;

namespace MapOptions;

public static class PlayerMod
{
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
                //On.Player.ctor += Player_ctor; // add or update slugcat symbol // maps not initialized yet => does nothing
                On.Player.Regurgitate += Player_Regurgitate; // add creature symbol if needed
                On.Player.SwallowObject += Player_SwallowObject; // remove creature symbol if needed
            }
            else
            {
                On.Player.Regurgitate -= Player_Regurgitate;
                On.Player.SwallowObject -= Player_SwallowObject;
            }
        }
    }

    //
    // public
    //

    public static void RemoveObjectInStomachSymbol(Player player)
    {
        if (player.objectInStomach is AbstractCreature abstractCreature)
        {
            foreach (MapMod.AttachedFields attachedFields in MapMod.all_attached_fields.Values)
            {
                foreach (Creature_Symbol_On_Map creature_symbol in attachedFields.creature_symbols)
                {
                    if (creature_symbol.abstract_creature == abstractCreature)
                    {
                        creature_symbol.Remove_Sprites();
                        attachedFields.creature_symbols.Remove(creature_symbol);
                        break;
                    }
                }
            }
        }
    }

    //
    // private
    //

    private static void Player_Regurgitate(On.Player.orig_Regurgitate orig, Player player)
    {
        if (player.objectInStomach is AbstractCreature abstractCreature && !AbstractCreatureMod.creatureTypeBlacklist.Contains(abstractCreature.creatureTemplate.type))
        {
            foreach (KeyValuePair<HUD.Map, MapMod.AttachedFields> map_attachedFields in MapMod.all_attached_fields)
            {
                map_attachedFields.Value.creature_symbols.Add(new Creature_Symbol_On_Map(abstractCreature, map_attachedFields.Key.inFrontContainer));
            }
        }
        orig(player);
    }

    private static void Player_SwallowObject(On.Player.orig_SwallowObject orig, Player player, int grasp)
    {
        orig(player, grasp);
        RemoveObjectInStomachSymbol(player);
    }
}