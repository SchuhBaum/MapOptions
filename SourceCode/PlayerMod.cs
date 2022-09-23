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
                    foreach (CreatureSymbolPair creatureSymbolPair in attachedFields.creatureSymbolList)
                    {
                        if (creatureSymbolPair.abstractCreature == abstractCreature)
                        {
                            creatureSymbolPair.RemoveSprites();
                            attachedFields.creatureSymbolList.Remove(creatureSymbolPair);
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
                CreatureSymbolPair creatureSymbolPair = new(abstractCreature);
                foreach (MapMod.AttachedFields attachedFields in MapMod.allAttachedFields.Values)
                {
                    attachedFields.creatureSymbolList.Add(creatureSymbolPair);
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