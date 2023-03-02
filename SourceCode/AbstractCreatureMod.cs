using System.Collections.Generic;

namespace MapOptions
{
    public static class AbstractCreatureMod
    {
        public static readonly List<CreatureTemplate.Type> creatureTypeBlacklist = new() { CreatureTemplate.Type.StandardGroundCreature, CreatureTemplate.Type.Fly, CreatureTemplate.Type.Slugcat };

        internal static void OnEnable()
        {
            On.AbstractCreature.ctor += AbstractCreature_ctor; // adds creature symbols // map might not be initialized yet => add when maps get initialized
        }

        internal static void OnDisable()
        {
            On.AbstractCreature.ctor -= AbstractCreature_ctor;
        }

        // ----------------- //
        // private functions //
        // ----------------- //

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
}