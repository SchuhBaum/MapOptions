using System.Collections.Generic;
using RWCustom;
using UnityEngine;

namespace MapOptions;

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

    internal static readonly Dictionary<HUD.Map, AttachedFields> allAttachedFields = new();
    public static AttachedFields? TryGetAttachedFields(this HUD.Map map)
    {
        allAttachedFields.TryGetValue(map, out AttachedFields? attachedFields);
        return attachedFields;
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

    //
    // public
    //

    public static void UncoverRoom(HUD.Map map, AbstractRoom abstractRoom)
    {
        if (map.discoverTexture == null) return;

        // increase the area of rooms;
        // otherwise some connections might not get immediately uncoverd;
        IntVector2 startPosition = IntVector2.FromVector2(map.OnTexturePos(new Vector2(-60f, -60f), abstractRoom.index, accountForLayer: true) / map.DiscoverResolution);
        IntVector2 endPosition = IntVector2.FromVector2(map.OnTexturePos(abstractRoom.size.ToVector2() * 20f + new Vector2(60f, 60f), abstractRoom.index, accountForLayer: true) / map.DiscoverResolution);

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

    //
    // private
    //

    private static float HUD_Map_Alpha(On.HUD.Map.orig_Alpha orig, HUD.Map map, int layer, float timeStacker, bool compensateForLayersInFront)
    {
        if (!MainMod.Option_LayerFocus)
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
        // I am confused
        // this function detaches / deletes inFrontContainer and other suff; but does not destroys map like HUD.ResetMap() does
        // inFrontContainer is only set in ctor => map is an empty hull(?) anyways
        // maybe there are NullRef problems otherwise
        orig(map);
        if (map.TryGetAttachedFields() is not AttachedFields attachedFields) return;

        foreach (Creature_Symbol_On_Map creatureSymbol in attachedFields.creatureSymbols)
        {
            creatureSymbol.Remove_Sprites();
        }

        foreach (Creature_Symbol_On_Map slugcatSymbol in attachedFields.slugcatSymbols)
        {
            slugcatSymbol.Remove_Sprites();
        }

        attachedFields.creatureSymbols.Clear();
        attachedFields.slugcatSymbols.Clear();
        allAttachedFields.Remove(map);
    }

    private static void HUD_Map_ctor(On.HUD.Map.orig_ctor orig, HUD.Map map, HUD.HUD hud, HUD.Map.MapData mapData)
    {
        orig(map, hud, mapData);

        // no arial maps
        if (!MainMod.Option_AerialMap)
        {
            map.STANDARDELEMENTLIST[1] = false;
            map.STANDARDELEMENTLIST[2] = false;
        }

        HUD.HUD.OwnerType ownerType = map.hud.owner.GetOwnerType();
        if (ownerType != HUD.HUD.OwnerType.Player && ownerType != HUD.HUD.OwnerType.SleepScreen && ownerType != HUD.HUD.OwnerType.DeathScreen) return;

        AttachedFields attachedFields = new();
        if (map.hud.rainWorld.processManager.currentMainLoop is RainWorldGame game && game.IsStorySession)
        {
            // add creature and slugcat symbols of creatures that were already created
            if (MainMod.Option_CreatureSymbols)
            {
                foreach (AbstractRoom abstractRoom in game.world.abstractRooms)
                {
                    // not in dens
                    foreach (AbstractCreature abstractCreature in abstractRoom.creatures)
                    {
                        if (!AbstractCreatureMod.creatureTypeBlacklist.Contains(abstractCreature.creatureTemplate.type))
                        {
                            attachedFields.creatureSymbols.Add(new Creature_Symbol_On_Map(abstractCreature, map.inFrontContainer));
                        }
                    }

                    // in dens
                    foreach (AbstractWorldEntity abstractWorldEntity in abstractRoom.entitiesInDens)
                    {
                        if (abstractWorldEntity is AbstractCreature abstractCreature && !AbstractCreatureMod.creatureTypeBlacklist.Contains(abstractCreature.creatureTemplate.type))
                        {
                            attachedFields.creatureSymbols.Add(new Creature_Symbol_On_Map(abstractCreature, map.inFrontContainer));
                        }
                    }
                }

                foreach (AbstractCreature abstractPlayer in game.Players)
                {
                    if (abstractPlayer.realizedCreature is Player player)
                    {
                        PlayerMod.RemoveObjectInStomachSymbol(player);
                    }
                }
            }

            if (MainMod.Option_SlugcatSymbols)
            {
                foreach (AbstractCreature abstractPlayer in game.Players)
                {
                    attachedFields.slugcatSymbols.Add(new Creature_Symbol_On_Map(abstractPlayer, map.inFrontContainer));
                }
            }
        }
        allAttachedFields.Add(map, attachedFields);
    }

    private static void HUD_Map_Draw(On.HUD.Map.orig_Draw orig, HUD.Map map, float timeStacker)
    {
        if (map.slatedForDeletion || map.TryGetAttachedFields() is not AttachedFields attachedFields)
        {
            orig(map, timeStacker);
            return;
        }

        if (MainMod.Option_LayerFocus && map.visible)
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
            if (MainMod.Option_CreatureSymbols)
            {
                Dictionary<AbstractRoom, List<Creature_Symbol_On_Map>> creatureSymbolPerRoomList = new();
                Dictionary<AbstractRoom, List<CreatureTemplate.Type>> creatureTypePerRoomList = new();

                // sort creatures per room // only add one symbol of each creature type per room
                for (int symbolIndex = attachedFields.creatureSymbols.Count - 1; symbolIndex >= 0; --symbolIndex)
                {
                    Creature_Symbol_On_Map creatureSymbol = attachedFields.creatureSymbols[symbolIndex];
                    if (creatureSymbol.Is_Creature_Dead) // don't show dead creatures
                    {
                        creatureSymbol.Remove_Sprites();
                        attachedFields.creatureSymbols.RemoveAt(symbolIndex);
                    }
                    else if (creatureSymbol.Is_Creature_In_Den)
                    {
                        // hide creature in dens
                        creatureSymbol.Is_Visible = false;
                    }
                    else if (creatureSymbol.Abstract_Room != null)
                    {
                        if (!creatureSymbolPerRoomList.ContainsKey(creatureSymbol.Abstract_Room))
                        {
                            creatureSymbolPerRoomList.Add(creatureSymbol.Abstract_Room, new List<Creature_Symbol_On_Map>());
                            creatureTypePerRoomList.Add(creatureSymbol.Abstract_Room, new List<CreatureTemplate.Type>());
                        }

                        if (!creatureTypePerRoomList[creatureSymbol.Abstract_Room].Contains(creatureSymbol.abstract_creature.creatureTemplate.type))
                        {
                            creatureSymbolPerRoomList[creatureSymbol.Abstract_Room].Add(creatureSymbol);
                            creatureTypePerRoomList[creatureSymbol.Abstract_Room].Add(creatureSymbol.abstract_creature.creatureTemplate.type);
                        }
                        else
                        {
                            // might hide creature when they enter rooms
                            creatureSymbol.Is_Visible = false;
                        }
                    }
                }

                // display symbols // iterate over rooms // iterate over creatures in room
                foreach (AbstractRoom abstractRoom in creatureSymbolPerRoomList.Keys)
                {
                    float roomWidthPerCreature = map.mapData.SizeOfRoom(abstractRoom.index).x / (creatureSymbolPerRoomList[abstractRoom].Count + 1f);
                    for (int symbolIndex = 0; symbolIndex < creatureSymbolPerRoomList[abstractRoom].Count; ++symbolIndex)
                    {
                        Creature_Symbol_On_Map creatureSymbol = creatureSymbolPerRoomList[abstractRoom][symbolIndex];
                        Vector2 inRoomPos = new(roomWidthPerCreature * (symbolIndex + 1) * 20f, map.mapData.SizeOfRoom(abstractRoom.index).y * 10f);
                        IntVector2 onRevealTexturePos = IntVector2.FromVector2(map.OnTexturePos(inRoomPos, abstractRoom.index, true) / map.DiscoverResolution);

                        if (map.revealTexture.GetPixel(onRevealTexturePos.x, onRevealTexturePos.y).r < 0.5f)
                        {
                            // hide symbols that are not revealed
                            creatureSymbol.Is_Visible = false;
                        }
                        else
                        {
                            creatureSymbol.Draw(map, timeStacker, inRoomPos - new Vector2(10f, 10f));
                            creatureSymbol.Is_Visible = true;
                        }
                    }
                }
            }

            if (MainMod.Option_SlugcatSymbols)
            {
                foreach (Creature_Symbol_On_Map slugcatSymbol in attachedFields.slugcatSymbols)
                {
                    if (slugcatSymbol.abstract_creature.realizedCreature is Player player && player.room != null)
                    {
                        IntVector2 onRevealTexturePos = IntVector2.FromVector2(map.OnTexturePos(player.mainBodyChunk.pos, slugcatSymbol.Abstract_Room.index, true) / map.DiscoverResolution);
                        if (map.revealTexture.GetPixel(onRevealTexturePos.x, onRevealTexturePos.y).r < 0.5f)
                        {
                            // hide symbols that are not revealed // can only happen in multiplayer
                            slugcatSymbol.Is_Visible = false;
                        }
                        else
                        {
                            slugcatSymbol.Draw(map, timeStacker, player.mainBodyChunk.pos - new Vector2(10f, 10f));
                            slugcatSymbol.Is_Visible = true;
                        }
                    }
                    else
                    {
                        // hide non-realized player // like in jollycoop when they are dead and in offscreen den
                        slugcatSymbol.Is_Visible = false;
                    }
                }
            }
        }
        else if (!attachedFields.hasMapClosed)
        {
            attachedFields.hasMapClosed = true;
            foreach (Creature_Symbol_On_Map creatureSymbol in attachedFields.creatureSymbols)
            {
                creatureSymbol.Is_Visible = false;
            }

            foreach (Creature_Symbol_On_Map slugcatSymbol in attachedFields.slugcatSymbols)
            {
                slugcatSymbol.Is_Visible = false;
            }
        }
    }

    private static void HUD_Map_Update(On.HUD.Map.orig_Update orig, HUD.Map map)
    {
        if (map.slatedForDeletion || map.TryGetAttachedFields() is not AttachedFields attachedFields)
        {
            orig(map);
            return;
        }

        if (map.discLoaded && uncoveredRooms.Count > 0)
        {
            UncoverRoom(map, uncoveredRooms[0]);
            uncoveredRooms.RemoveAt(0);
        }

        if (MainMod.Option_SkipFade && map.mapLoaded && map.discLoaded)
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

            if (MainMod.Option_SlugcatSymbols)
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

    //
    //
    //

    public sealed class AttachedFields
    {
        public bool hasMapClosed;
        public bool hasMapLoaded;
        public readonly List<Creature_Symbol_On_Map> creatureSymbols = new();
        public readonly List<Creature_Symbol_On_Map> slugcatSymbols = new();
    }
}