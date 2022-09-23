using System.Collections.Generic;
using RWCustom;
using UnityEngine;

namespace MapOptions
{
    public static class MapMod
    {
        //
        // parameters
        //

        public static float creatureSymbolScale = 1f;
        public static float mapScale = 1f;
        public static float slugcatSymbolScale = 1f;
        public static int revealSpeedMultiplier = 1;

        public static bool IsScalingEnabled => !(mapScale == 1f);
        public static bool CanInstantReveal => revealSpeedMultiplier == 10;

        //
        // variables
        //

        public sealed class AttachedFields
        {
            public bool hasMapClosed;
            public bool hasMapLoaded;
            public readonly List<CreatureSymbolPair> creatureSymbolList = new();
            public readonly List<CreatureSymbolPair> slugcatSymbols = new();
        }
        internal static readonly Dictionary<HUD.Map, AttachedFields> allAttachedFields = new();
        public static AttachedFields? GetAttachedFields(this HUD.Map map)
        {
            if (map.hud.owner.GetOwnerType() != HUD.HUD.OwnerType.Player)
            {
                return null;
            }
            return allAttachedFields[map];
        }

        public static List<AbstractRoom> uncoveredRooms = new();

        internal static void OnEnable()
        {
            On.HUD.Map.Alpha += HUD_Map_Alpha;
            On.HUD.Map.ClearSprites += HUD_Map_ClearSprites;
            On.HUD.Map.ctor += HUD_Map_ctor;

            On.HUD.Map.Draw += HUD_Map_Draw;
            On.HUD.Map.Update += HUD_Map_Update;
        }

        // what is this system?
        // revealPixels seem to influence the spreading of the reveal
        // revealFadePixels seem to influence the transistion of data from discoverTexture to revealTexture
        // discoverTexture is stable and updated when you walk around // revealTexture gets reset after moving slugcat
        // problem: the reveal effect can die out // sometimes re-opening the map shows the rest // problem can be "solved" by only discovering the rooms and not the spaces inbetween

        // ---------------- //
        // public functions //
        // ---------------- //

        public static void ClearAttachedFields(AttachedFields attachedFields)
        {
            for (int creatureIndex = attachedFields.creatureSymbolList.Count - 1; creatureIndex >= 0; --creatureIndex)
            {
                attachedFields.creatureSymbolList[creatureIndex].RemoveSprites();
                attachedFields.creatureSymbolList.RemoveAt(creatureIndex);
            }

            for (int slugcatIndex = attachedFields.slugcatSymbols.Count - 1; slugcatIndex >= 0; --slugcatIndex)
            {
                attachedFields.slugcatSymbols[slugcatIndex].RemoveSprites();
                attachedFields.slugcatSymbols.RemoveAt(slugcatIndex);
            }

            attachedFields.hasMapClosed = false;
            attachedFields.hasMapLoaded = false;
        }

        public static void UncoverRoom(HUD.Map map, AbstractRoom abstractRoom)
        {
            if (map.discoverTexture == null)
            {
                return;
            }

            IntVector2 startPosition = IntVector2.FromVector2(map.OnTexturePos(new Vector2(0.0f, 0.0f), abstractRoom.index, accountForLayer: true) / map.DiscoverResolution);
            IntVector2 endPosition = IntVector2.FromVector2(map.OnTexturePos(abstractRoom.size.ToVector2() * 20f, abstractRoom.index, accountForLayer: true) / map.DiscoverResolution);

            if (map.discoverTexture.GetPixel(startPosition.x, startPosition.y).r == 0.0f ||
                map.discoverTexture.GetPixel(startPosition.x, endPosition.y).r == 0.0f ||
                map.discoverTexture.GetPixel(endPosition.x, startPosition.y).r == 0.0f ||
                map.discoverTexture.GetPixel(endPosition.x, endPosition.y).r == 0.0f ||
                map.discoverTexture.GetPixel(Random.Range(startPosition.x, endPosition.x), Random.Range(startPosition.y, endPosition.y)).r == 0.0f)
            {
                for (int x = startPosition.x; x < endPosition.x; x++)
                {
                    for (int y = startPosition.y; y < endPosition.y; y++)
                    {
                        map.discoverTexture.SetPixel(x, y, new Color(1f, 1f, 1f));
                    }
                }
            }
        }

        // ----------------- //
        // private functions //
        // ----------------- //

        private static float HUD_Map_Alpha(On.HUD.Map.orig_Alpha orig, HUD.Map map, int layer, float timeStacker, bool compensateForLayersInFront)
        {
            if (!MainMod.onlyActiveLayerOption)
            {
                return orig(map, layer, timeStacker, compensateForLayersInFront);
            }
            else if (layer == map.depth) // no fade in/out effect when switching layers
            {
                return Mathf.Lerp(map.lastFade, map.fade, timeStacker);
            }
            else
            {
                return 0.0f;
            }
        }

        private static void HUD_Map_ClearSprites(On.HUD.Map.orig_ClearSprites orig, HUD.Map map)
        {
            orig(map);
            AttachedFields? attachedFields = map.GetAttachedFields();

            if (attachedFields == null)
            {
                return;
            }

            foreach (CreatureSymbolPair creatureSymbolPair in attachedFields.creatureSymbolList)
            {
                creatureSymbolPair.RemoveSprites();
            }

            foreach (CreatureSymbolPair slugcatSymbolPair in attachedFields.slugcatSymbols)
            {
                slugcatSymbolPair.RemoveSprites();
            }
        }

        private static void HUD_Map_ctor(On.HUD.Map.orig_ctor orig, HUD.Map map, HUD.HUD hud, HUD.Map.MapData mapData)
        {
            orig(map, hud, mapData);
            if (map.hud.owner.GetOwnerType() != HUD.HUD.OwnerType.Player)
            {
                return;
            }

            AttachedFields attachedFields = new();
            if (map.hud.rainWorld.processManager.currentMainLoop is RainWorldGame game && game.IsStorySession)
            {
                // add creature and slugcat symbols of creatures that were already created
                if (MainMod.creatureSymbolsOption)
                {
                    foreach (AbstractRoom abstractRoom in game.world.abstractRooms)
                    {
                        foreach (AbstractWorldEntity abstractWorldEntity in abstractRoom.entitiesInDens)
                        {
                            if (abstractWorldEntity is AbstractCreature abstractCreature && !AbstractCreatureMod.creatureTypeBlacklist.Contains(abstractCreature.creatureTemplate.type))
                            {
                                attachedFields.creatureSymbolList.Add(new CreatureSymbolPair(abstractCreature));
                            }
                        }

                        foreach (AbstractWorldEntity abstractWorldEntity in abstractRoom.entities)
                        {
                            if (abstractWorldEntity is AbstractCreature abstractCreature && !AbstractCreatureMod.creatureTypeBlacklist.Contains(abstractCreature.creatureTemplate.type))
                            {
                                attachedFields.creatureSymbolList.Add(new CreatureSymbolPair(abstractCreature));
                            }
                        }
                    }
                }

                if (MainMod.slugcatSymbolsOption)
                {
                    foreach (AbstractCreature abstractPlayer in game.Players)
                    {
                        attachedFields.slugcatSymbols.Add(new CreatureSymbolPair(abstractPlayer));
                        if (abstractPlayer.realizedCreature is Player player)
                        {
                            PlayerMod.RemoveObjectInStomachSymbol(player);
                        }
                    }
                }
            }
            allAttachedFields.Add(map, attachedFields);
        }

        private static void HUD_Map_Draw(On.HUD.Map.orig_Draw orig, HUD.Map map, float timeStacker)
        {
            AttachedFields? attachedFields = map.GetAttachedFields();
            if (attachedFields == null || map.slatedForDeletion)
            {
                orig(map, timeStacker);
                return;
            }

            if (MainMod.onlyActiveLayerOption && map.visible)
            {
                map.depth = map.layer;
                map.lastDepth = map.depth;

                if (map.playerMarker != null && map.playerMarker.sprite != null)
                {
                    map.playerMarker.fade = map.Alpha(map.mapData.LayerOfRoom(map.hud.owner.MapOwnerRoom), timeStacker, false);
                }

                foreach (HUD.Map.SwarmCircle swarmCircle in map.swarmCircles)
                {
                    if (swarmCircle.circle != null)
                    {
                        swarmCircle.circle.fade = map.Alpha(map.mapData.LayerOfRoom(swarmCircle.room), timeStacker, false);
                    }
                }

                foreach (HUD.Map.MapObject mapObject in map.mapObjects)
                {
                    if (mapObject is HUD.Map.ShelterMarker shelterMarker)
                    {
                        float alpha = map.Alpha(map.mapData.LayerOfRoom(shelterMarker.room), timeStacker, false);
                        shelterMarker.fade = alpha;

                        foreach (HUD.Map.ShelterMarker.ItemInShelterMarker itemInShelterMarker in shelterMarker.items)
                        {
                            if (itemInShelterMarker.symbol != null && itemInShelterMarker.symbol.symbolSprite != null)
                            {
                                itemInShelterMarker.symbol.symbolSprite.alpha = alpha;
                            }
                        }
                    }
                    else if (mapObject is HUD.Map.FadeInMarker fadeInMarker)
                    {
                        fadeInMarker.fade = map.Alpha(map.mapData.LayerOfRoom(fadeInMarker.room), timeStacker, false);
                    }
                }
            }
            orig(map, timeStacker);

            if (map.visible)
            {
                attachedFields.hasMapClosed = false;
                if (MainMod.creatureSymbolsOption)
                {
                    Dictionary<AbstractRoom, List<CreatureSymbolPair>> creatureSymbolPerRoomList = new();
                    Dictionary<AbstractRoom, List<CreatureTemplate.Type>> creatureTypePerRoomList = new();

                    // sort creatures per room // only add one symbol of each creature type per room
                    foreach (CreatureSymbolPair creatureSymbolPair in attachedFields.creatureSymbolList.ToArray())
                    {
                        if (creatureSymbolPair.IsCreatureDead) // don't show dead creatures
                        {
                            creatureSymbolPair.RemoveSprites();
                            attachedFields.creatureSymbolList.Remove(creatureSymbolPair);
                        }
                        else if (creatureSymbolPair.IsCreatureInDen)
                        {
                            // hide creature in dens
                            creatureSymbolPair.IsVisible = false;
                        }
                        else if (creatureSymbolPair.AbstractRoom != null)
                        {
                            if (!creatureSymbolPerRoomList.ContainsKey(creatureSymbolPair.AbstractRoom))
                            {
                                creatureSymbolPerRoomList.Add(creatureSymbolPair.AbstractRoom, new List<CreatureSymbolPair>());
                                creatureTypePerRoomList.Add(creatureSymbolPair.AbstractRoom, new List<CreatureTemplate.Type>());
                            }

                            if (!creatureTypePerRoomList[creatureSymbolPair.AbstractRoom].Contains(creatureSymbolPair.abstractCreature.creatureTemplate.type))
                            {
                                creatureSymbolPerRoomList[creatureSymbolPair.AbstractRoom].Add(creatureSymbolPair);
                                creatureTypePerRoomList[creatureSymbolPair.AbstractRoom].Add(creatureSymbolPair.abstractCreature.creatureTemplate.type);
                            }
                            else
                            {
                                // might hide creature when they enter rooms
                                creatureSymbolPair.IsVisible = false;
                            }
                        }
                    }

                    // display symbols // iterate over rooms // iterate over creatures in room
                    foreach (AbstractRoom abstractRoom in creatureSymbolPerRoomList.Keys)
                    {
                        float roomWidthPerCreature = map.mapData.SizeOfRoom(abstractRoom.index).x / (creatureSymbolPerRoomList[abstractRoom].Count + 1f);
                        for (int creatureSymbolIndex = 0; creatureSymbolIndex < creatureSymbolPerRoomList[abstractRoom].Count; ++creatureSymbolIndex)
                        {
                            CreatureSymbolPair creatureSymbolPair = creatureSymbolPerRoomList[abstractRoom][creatureSymbolIndex];
                            Vector2 inRoomPos = new(roomWidthPerCreature * (creatureSymbolIndex + 1) * 20f, map.mapData.SizeOfRoom(abstractRoom.index).y * 10f);
                            IntVector2 onRevealTexturePos = IntVector2.FromVector2(map.OnTexturePos(inRoomPos, abstractRoom.index, true) / map.DiscoverResolution);

                            if (map.revealTexture.GetPixel(onRevealTexturePos.x, onRevealTexturePos.y).r < 0.5f)
                            {
                                // hide symbols that are not revealed
                                creatureSymbolPair.IsVisible = false;
                            }
                            else
                            {
                                if (creatureSymbolPair.CreatureSymbol == null)
                                {
                                    creatureSymbolPair.CreateCreatureSymbol(map.inFrontContainer);
                                }

                                creatureSymbolPair.Draw(abstractRoom, map, timeStacker, inRoomPos);
                                creatureSymbolPair.IsVisible = true;
                            }
                        }
                    }
                }

                if (MainMod.slugcatSymbolsOption)
                {
                    foreach (CreatureSymbolPair slugcatSymbolPair in attachedFields.slugcatSymbols)
                    {
                        if (slugcatSymbolPair.abstractCreature.realizedCreature is Player player && player.room != null)
                        {
                            IntVector2 onRevealTexturePos = IntVector2.FromVector2(map.OnTexturePos(player.mainBodyChunk.pos, slugcatSymbolPair.AbstractRoom.index, true) / map.DiscoverResolution);
                            if (map.revealTexture.GetPixel(onRevealTexturePos.x, onRevealTexturePos.y).r < 0.5f)
                            {
                                // hide symbols that are not revealed // can only happen in multiplayer
                                slugcatSymbolPair.IsVisible = false;
                            }
                            else
                            {
                                if (slugcatSymbolPair.CreatureSymbol == null)
                                {
                                    slugcatSymbolPair.CreateCreatureSymbol(map.inFrontContainer);
                                }

                                slugcatSymbolPair.Draw(slugcatSymbolPair.AbstractRoom, map, timeStacker, player.mainBodyChunk.pos - new Vector2(20f, 0.0f));
                                slugcatSymbolPair.IsVisible = true;
                            }
                        }
                        else
                        {
                            // hide non-realized player // like in jollycoop when they are dead and in offscreen den
                            slugcatSymbolPair.IsVisible = false;
                        }
                    }
                }
            }
            else if (!attachedFields.hasMapClosed)
            {
                attachedFields.hasMapClosed = true;
                foreach (CreatureSymbolPair creatureSymbolPair in attachedFields.creatureSymbolList)
                {
                    creatureSymbolPair.IsVisible = false;
                }

                foreach (CreatureSymbolPair slugcatSymbolPair in attachedFields.slugcatSymbols)
                {
                    slugcatSymbolPair.IsVisible = false;
                }
            }
        }

        private static void HUD_Map_Update(On.HUD.Map.orig_Update orig, HUD.Map map)
        {
            AttachedFields? attachedFields = map.GetAttachedFields();
            if (attachedFields == null || map.slatedForDeletion)
            {
                orig(map);
                return;
            }

            if (map.discLoaded && uncoveredRooms.Count > 0)
            {
                UncoverRoom(map, uncoveredRooms[0]);
                uncoveredRooms.RemoveAt(0);
            }

            if (MainMod.skipFadeOption && map.mapLoaded && map.discLoaded)
            {
                if (map.hud.owner.RevealMap)
                {
                    map.fadeCounter = 31;
                    map.fade = 1f;

                    if (map.lastFade == 0.0)
                    {
                        map.InitiateMapView();
                        if (map.revealAllDiscovered)
                        {
                            map.RevealAllDiscovered();
                        }
                    }
                }
                else
                {
                    map.fadeCounter = 0;
                    map.fade = 0.0f;
                    map.lastFade = 0.0f;
                }
            }
            orig(map);

            if (!attachedFields.hasMapLoaded && map.discLoaded)
            {
                if (IsScalingEnabled)
                {
                    map.MapScale /= mapScale; // this controls how "deep" (front/back) the icons are placed; needs to align with the texture;
                    Shader.SetGlobalVector("_mapSize", map.mapSize / mapScale);
                }

                if (MainMod.slugcatSymbolsOption)
                {
                    map.playerMarker?.ClearSprite();
                    map.playerMarker = null;
                }

                map.revealAllDiscovered = CanInstantReveal;
                attachedFields.hasMapLoaded = true;
            }

            if (map.mapLoaded && map.discLoaded && revealSpeedMultiplier > 1 && !CanInstantReveal)
            {
                for (int _ = 1; _ < revealSpeedMultiplier - 1; _++)
                {
                    if (map.revealPixelsList.Count > 0)
                    {
                        map.RevealRoutine();
                    }

                    if (map.revealFadePixels.Count > 0)
                    {
                        map.FadeRoutine();
                    }
                }
            }
        }
    }
}