using System.Collections.Generic;

namespace MapOptions
{
    public static class PlayerMod
    {
        internal static void OnEnable()
        {
            //On.Player.ctor += Player_ctor; // add or update slugcat symbol // maps not initialized yet => does nothing
            On.Player.Regurgitate += Player_Regurgitate; // add creature symbol if needed
            On.Player.SwallowObject += Player_SwallowObject; // remove creature symbol if needed
        }

        internal static void OnDisable()
        {
            On.Player.Regurgitate -= Player_Regurgitate;
            On.Player.SwallowObject -= Player_SwallowObject;
        }

        // ---------------- //
        // public functions //
        // ---------------- //

        public static void RemoveObjectInStomachSymbol(Player player)
        {
            if (player.objectInStomach is AbstractCreature abstractCreature)
            {
                foreach (MapMod.AttachedFields attachedFields in MapMod.allAttachedFields.Values)
                {
                    foreach (Creature_Symbol_On_Map creatureSymbol in attachedFields.creatureSymbols)
                    {
                        if (creatureSymbol.abstract_creature == abstractCreature)
                        {
                            creatureSymbol.Remove_Sprites();
                            attachedFields.creatureSymbols.Remove(creatureSymbol);
                            break;
                        }
                    }
                }
            }
        }

        // ----------------- //
        // private functions //
        // ----------------- //

        private static void Player_Regurgitate(On.Player.orig_Regurgitate orig, Player player)
        {
            if (player.objectInStomach is AbstractCreature abstractCreature && !AbstractCreatureMod.creatureTypeBlacklist.Contains(abstractCreature.creatureTemplate.type))
            {
                foreach (KeyValuePair<HUD.Map, MapMod.AttachedFields> map_attachedFields in MapMod.allAttachedFields)
                {
                    map_attachedFields.Value.creatureSymbols.Add(new Creature_Symbol_On_Map(abstractCreature, map_attachedFields.Key.inFrontContainer));
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
}