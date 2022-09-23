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
                foreach (HUD.Map map in MapMod.creatureSymbolList.Keys)
                {
                    foreach (CreatureSymbolPair creatureSymbolPair_ in MapMod.creatureSymbolList[map].ToArray())
                    {
                        if (creatureSymbolPair_.abstractCreature == abstractCreature)
                        {
                            creatureSymbolPair_.RemoveSprites();
                            MapMod.creatureSymbolList[map].Remove(creatureSymbolPair_);
                            break;
                        }
                    }
                }
            }
        }

        // ----------------- //
        // private functions //
        // ----------------- //

        //private static void Player_ctor(On.Player.orig_ctor orig, Player player, AbstractCreature abstractCreature, World world)
        //{
        //    orig(player, abstractCreature, world);
        //    if (MainMod.slugcatSymbolsEnabled)
        //    {
        //        int playerNumber = player.playerState.playerNumber;
        //        foreach (HUD.Map map in MapMod.slugcatSymbols.Keys)
        //        {
        //            if (MapMod.slugcatSymbols[map][playerNumber] is CreatureSymbolPair creatureSymbolPair)
        //            {
        //                creatureSymbolPair.RemoveSprites();
        //            }
        //            MapMod.slugcatSymbols[map][playerNumber] = new CreatureSymbolPair(player.abstractCreature);
        //        }
        //    }
        //    RemoveObjectInStomachSymbol(player);
        //}

        private static void Player_Regurgitate(On.Player.orig_Regurgitate orig, Player player)
        {
            if (player.objectInStomach is AbstractCreature abstractCreature && !AbstractCreatureMod.creatureTypeBlacklist.Contains(abstractCreature.creatureTemplate.type))
            {
                CreatureSymbolPair creatureSymbolPair = new CreatureSymbolPair(abstractCreature);
                foreach (HUD.Map map in MapMod.creatureSymbolList.Keys)
                {
                    if (!MapMod.creatureSymbolList[map].Contains(creatureSymbolPair)) // what is needed for creatureSymbol to be considered equal? // does visibility matter?
                    {
                        MapMod.creatureSymbolList[map].Add(creatureSymbolPair);
                    }
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