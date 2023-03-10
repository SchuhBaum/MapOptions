using System.Collections.Generic;
using HUD;
using MonoMod.Cil;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

using static HUD.HUD;
using static HUD.Map;
using static MapOptions.MainMod;
using static MapOptions.MainModOptions;

namespace MapOptions;

public static class MapMod
{
    //
    // parameters
    //

    public static bool Is_Scaling_Enabled => !(Map_Scale == 1f);
    public static bool Can_Instant_Reveal => Reveal_Speed_Multiplier == 10;

    public static float Creature_Symbols_Scale => creature_symbol_scale.Value / 10f;
    public static float Map_Scale => 10f / zoom_slider.Value;
    public static float Slugcat_Symbols_Scale => slugcat_symbol_scale.Value / 10f;

    public static int Reveal_Speed_Multiplier => reveal_speed_multiplier.Value;

    //
    // variables
    //

    internal static readonly Dictionary<Map, AttachedFields> all_attached_fields = new();
    public static AttachedFields? Get_Attached_Fields(this Map map)
    {
        all_attached_fields.TryGetValue(map, out AttachedFields? attachedFields);
        return attachedFields;
    }

    private static bool is_enabled = false;

    public static List<AbstractRoom> uncovered_rooms = new();

    //
    //
    //

    // this needs to be separate;
    // the map is active outside of the game process;
    // OnToggle() is not enough;
    internal static void OnEnable()
    {
        On.HUD.Map.Alpha += HUD_Map_Alpha;
        On.HUD.Map.ClearSprites += HUD_Map_ClearSprites;
        On.HUD.Map.ctor += HUD_Map_ctor;

        On.HUD.Map.Draw += HUD_Map_Draw;
        On.HUD.Map.Update += HUD_Map_Update;
    }

    internal static void OnToggle()
    {
        is_enabled = !is_enabled;
        if (Option_ItemTracker)
        {
            if (is_enabled)
            {
                IL.HUD.Map.ItemMarker.Draw += IL_HUD_Map_ItemMarker_Draw;
            }
            else
            {
                IL.HUD.Map.ItemMarker.Draw -= IL_HUD_Map_ItemMarker_Draw;
            }
        }
    }

    // what is this system?
    // revealPixels seem to influence the spreading of the reveal
    // revealFadePixels seem to influence the transistion of data from discoverTexture to revealTexture
    // discoverTexture is stable and updated when you walk around // revealTexture gets reset after moving slugcat
    // problem: the reveal effect can die out // sometimes re-opening the map shows the rest // problem can be "solved" by only discovering the rooms and not the spaces inbetween

    //
    // public
    //

    public static void Draw_Creature_Symbols(Map map, float time_stacker)
    {
        if (!Option_CreatureSymbols) return;
        if (map.Get_Attached_Fields() is not AttachedFields attached_fields) return;

        Dictionary<AbstractRoom, List<Creature_Symbol_On_Map>> creature_symbols_sorted_by_room = new();
        Dictionary<AbstractRoom, List<CreatureTemplate.Type>> creature_types_sorted_by_room = new();

        // sort
        for (int symbolIndex = attached_fields.creature_symbols.Count - 1; symbolIndex >= 0; --symbolIndex)
        {
            Creature_Symbol_On_Map creature_symbol = attached_fields.creature_symbols[symbolIndex];
            if (creature_symbol.Is_Creature_Dead) // don't show dead creatures
            {
                creature_symbol.Remove_Sprites();
                attached_fields.creature_symbols.RemoveAt(symbolIndex);
                continue;
            }

            if (creature_symbol.Is_Creature_In_Den)
            {
                creature_symbol.Is_Visible = false;
                continue;
            }

            AbstractRoom abstract_room = creature_symbol.Abstract_Room;
            if (!creature_symbols_sorted_by_room.ContainsKey(abstract_room))
            {
                creature_symbols_sorted_by_room.Add(abstract_room, new());
            }

            if (!creature_types_sorted_by_room.ContainsKey(abstract_room))
            {
                creature_types_sorted_by_room.Add(abstract_room, new());
            }

            List<Creature_Symbol_On_Map> creature_symbols_in_room = creature_symbols_sorted_by_room[abstract_room];
            List<CreatureTemplate.Type> creature_types_in_room = creature_types_sorted_by_room[abstract_room];
            CreatureTemplate.Type creature_type = creature_symbol.abstract_creature.creatureTemplate.type;

            if (creature_types_in_room.Contains(creature_type))
            {
                creature_symbol.Is_Visible = false;
                continue;
            }

            creature_symbols_in_room.Add(creature_symbol);
            creature_types_in_room.Add(creature_type);
        }

        // draw
        foreach (AbstractRoom abstract_room in creature_symbols_sorted_by_room.Keys)
        {
            // in GW when using spearmaster there are rooms that are disabled but creature still spawn;
            if (abstract_room.world.DisabledMapRooms.Contains(abstract_room.name)) continue;

            List<Creature_Symbol_On_Map> creature_symbols_in_room = creature_symbols_sorted_by_room[abstract_room];
            float room_width_per_creature = map.mapData.SizeOfRoom(abstract_room.index).x / (creature_symbols_in_room.Count + 1f);

            for (int symbol_index = 0; symbol_index < creature_symbols_in_room.Count; ++symbol_index)
            {
                Creature_Symbol_On_Map creature_symbol = creature_symbols_in_room[symbol_index];
                Vector2 in_room_position = new(room_width_per_creature * (symbol_index + 1) * 20f, map.mapData.SizeOfRoom(abstract_room.index).y * 10f);
                IntVector2 on_reveal_texture_position = IntVector2.FromVector2(map.OnTexturePos(in_room_position, abstract_room.index, true) / map.DiscoverResolution);

                if (map.revealTexture.GetPixel(on_reveal_texture_position.x, on_reveal_texture_position.y).r < 0.5f)
                {
                    // hide symbols that are not revealed
                    creature_symbol.Is_Visible = false;
                    continue;
                }

                creature_symbol.Draw(map, time_stacker, in_room_position - new Vector2(10f, 10f));
                creature_symbol.Is_Visible = true;
            }
        }
    }

    public static void Draw_Slugcat_Symbols(Map map, float time_stacker)
    {
        if (!Option_SlugcatSymbols) return;
        if (map.Get_Attached_Fields() is not AttachedFields attached_fields) return;

        foreach (Creature_Symbol_On_Map slugcat_symbol in attached_fields.slugcat_symbols)
        {
            if (slugcat_symbol.abstract_creature.realizedCreature is not Player player || player.room == null)
            {
                // hide non-realized player
                // like in jollycoop when they are dead and in offscreen den
                slugcat_symbol.Is_Visible = false;
                continue;
            }

            IntVector2 on_reveal_texture_position = IntVector2.FromVector2(map.OnTexturePos(player.mainBodyChunk.pos, slugcat_symbol.Abstract_Room.index, true) / map.DiscoverResolution);
            if (map.revealTexture.GetPixel(on_reveal_texture_position.x, on_reveal_texture_position.y).r < 0.5f)
            {
                // hide symbols that are not revealed
                // can only happen in multiplayer
                slugcat_symbol.Is_Visible = false;
                continue;
            }

            slugcat_symbol.Draw(map, time_stacker, player.mainBodyChunk.pos - new Vector2(10f, 10f));
            slugcat_symbol.Is_Visible = true;
        }
    }

    public static bool Has_Item_Marker(Map map, AbstractPhysicalObject item)
    {
        foreach (ItemMarker old_item_marker in map.itemMarkers)
        {
            if (old_item_marker.obj != item) continue;
            return true;
        }
        return false;
    }

    public static void Increase_Reveal_Speed(Map map)
    {
        if (Reveal_Speed_Multiplier == 1) return;
        if (Can_Instant_Reveal) return;
        if (!map.discLoaded) return;
        if (!map.mapLoaded) return;

        for (int _ = 1; _ < Reveal_Speed_Multiplier - 1; _++)
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

    public static void Initialize_Creature_Symbols(Map map, RainWorldGame game)
    {
        // add creature and slugcat symbols of creatures that were already created
        if (!Option_CreatureSymbols) return;
        if (map.Get_Attached_Fields() is not AttachedFields attachedFields) return;
        if (attachedFields.creature_symbols.Count > 0) return;

        foreach (AbstractRoom abstractRoom in game.world.abstractRooms)
        {
            // not in dens
            foreach (AbstractCreature abstractCreature in abstractRoom.creatures)
            {
                if (!AbstractCreatureMod.creatureTypeBlacklist.Contains(abstractCreature.creatureTemplate.type))
                {
                    attachedFields.creature_symbols.Add(new Creature_Symbol_On_Map(abstractCreature, map.inFrontContainer));
                }
            }

            // in dens
            foreach (AbstractWorldEntity abstractWorldEntity in abstractRoom.entitiesInDens)
            {
                if (abstractWorldEntity is AbstractCreature abstractCreature && !AbstractCreatureMod.creatureTypeBlacklist.Contains(abstractCreature.creatureTemplate.type))
                {
                    attachedFields.creature_symbols.Add(new Creature_Symbol_On_Map(abstractCreature, map.inFrontContainer));
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

    public static void Initialize_Map_Variables(Map map)
    {
        if (!map.discLoaded) return;
        if (map.Get_Attached_Fields() is not AttachedFields attached_fields) return;

        if (attached_fields.is_map_loaded) return;
        attached_fields.is_map_loaded = true;

        if (Is_Scaling_Enabled)
        {
            // this controls how "deep" (front/back) the icons are placed; 
            // needs to align with the texture;
            map.MapScale /= Map_Scale;
            Shader.SetGlobalVector("_mapSize", map.mapSize / Map_Scale);
        }

        if (Option_SlugcatSymbols)
        {
            map.playerMarker?.ClearSprite();
            map.playerMarker = null;
        }
        map.revealAllDiscovered = Can_Instant_Reveal;
    }

    public static void Initialize_Slugcat_Symbols(Map map, RainWorldGame game)
    {
        if (!Option_SlugcatSymbols) return;
        if (map.Get_Attached_Fields() is not AttachedFields attachedFields) return;
        if (attachedFields.slugcat_symbols.Count > 0) return;

        foreach (AbstractCreature abstractPlayer in game.Players)
        {
            attachedFields.slugcat_symbols.Add(new Creature_Symbol_On_Map(abstractPlayer, map.inFrontContainer));
        }
    }

    public static void Focus_Layer(Map map, float time_stacker)
    {
        if (!Option_LayerFocus) return;
        if (map.visible) return;

        map.depth = map.layer;
        map.lastDepth = map.depth;

        if (map.playerMarker?.sprite != null)
        {
            map.playerMarker.fade = map.Alpha(map.mapData.LayerOfRoom(map.hud.owner.MapOwnerRoom), time_stacker, false);
        }

        foreach (SwarmCircle swarm_circle in map.swarmCircles)
        {
            if (swarm_circle.circle == null) continue;
            swarm_circle.circle.fade = map.Alpha(map.mapData.LayerOfRoom(swarm_circle.room), time_stacker, false);
        }

        foreach (MapObject map_object in map.mapObjects)
        {
            if (map_object is FadeInMarker fade_in_marker)
            {
                fade_in_marker.fade = map.Alpha(map.mapData.LayerOfRoom(fade_in_marker.room), time_stacker, false);
                continue;
            }

            if (map_object is not ShelterMarker shelter_marker) continue;
            float alpha = map.Alpha(map.mapData.LayerOfRoom(shelter_marker.room), time_stacker, false);
            shelter_marker.fade = alpha;

            foreach (ShelterMarker.ItemInShelterMarker item_in_shelter_marker in shelter_marker.items)
            {
                if (item_in_shelter_marker.symbol?.symbolSprite is not FSprite symbol_sprite) continue;
                symbol_sprite.alpha = alpha;
            }
        }
    }

    public static void Skip_Fade(Map map)
    {
        if (!Option_SkipFade) return;
        if (!map.mapLoaded) return;
        if (!map.discLoaded) return;

        if (!map.hud.owner.RevealMap)
        {
            map.fadeCounter = 0;
            map.fade = 0.0f;
            map.lastFade = 0.0f;
            return;
        }

        map.fadeCounter = 31;
        map.fade = 1f;

        if (map.lastFade != 0.0f) return;
        map.InitiateMapView();

        if (!map.revealAllDiscovered) return;
        map.RevealAllDiscovered();
    }

    public static void Uncover_Room(Map map, AbstractRoom abstract_room)
    {
        if (map.discoverTexture == null) return;

        // increase the area of rooms;
        // otherwise some connections might not get immediately uncoverd;
        IntVector2 startPosition = IntVector2.FromVector2(map.OnTexturePos(new Vector2(-60f, -60f), abstract_room.index, accountForLayer: true) / map.DiscoverResolution);
        IntVector2 endPosition = IntVector2.FromVector2(map.OnTexturePos(abstract_room.size.ToVector2() * 20f + new Vector2(60f, 60f), abstract_room.index, accountForLayer: true) / map.DiscoverResolution);

        if (map.discoverTexture.GetPixel(startPosition.x, startPosition.y).r == 0.0f ||
            map.discoverTexture.GetPixel(startPosition.x, endPosition.y).r == 0.0f ||
            map.discoverTexture.GetPixel(endPosition.x, startPosition.y).r == 0.0f ||
            map.discoverTexture.GetPixel(endPosition.x, endPosition.y).r == 0.0f ||
            map.discoverTexture.GetPixel(UnityEngine.Random.Range(startPosition.x, endPosition.x), UnityEngine.Random.Range(startPosition.y, endPosition.y)).r == 0.0f)
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

    private static void IL_HUD_Map_ItemMarker_Draw(ILContext context) // Option_ItemTracker
    {
        // LogAllInstructions(context);

        ILCursor cursor = new(context);
        if (cursor.TryGotoNext(instruction => instruction.MatchLdsfld<MMF>("cfgCreatureSense")))
        {
            // draw item markers even when slug senses are disabled;
            Debug.Log("MapOptions: IL_HUD_Map_ItemMarker_Draw: Index " + cursor.Index); // 3
            cursor.RemoveRange(3);
        }
        else
        {
            Debug.LogException(new System.Exception("MapOptions: IL_HUD_Map_ItemMarker_Draw failed."));
        }

        // LogAllInstructions(context);
    }

    //
    //
    //

    private static float HUD_Map_Alpha(On.HUD.Map.orig_Alpha orig, Map map, int layer, float time_stacker, bool compensate_for_layers_in_front)
    {
        if (!Option_LayerFocus)
        {
            return orig(map, layer, time_stacker, compensate_for_layers_in_front);
        }
        else if (layer == map.depth) // no fade in/out effect when switching layers
        {
            return Mathf.Lerp(map.lastFade, map.fade, time_stacker);
        }
        else
        {
            return 0.0f;
        }
    }

    private static void HUD_Map_ClearSprites(On.HUD.Map.orig_ClearSprites orig, Map map)
    {
        // I am confused
        // this function detaches / deletes inFrontContainer and other suff; but does not destroys map like ResetMap() does
        // inFrontContainer is only set in ctor => map is an empty hull(?) anyways
        // maybe there are NullRef problems otherwise
        orig(map);
        if (map.Get_Attached_Fields() is not AttachedFields attachedFields) return;

        foreach (Creature_Symbol_On_Map creature_symbol in attachedFields.creature_symbols)
        {
            creature_symbol.Remove_Sprites();
        }

        foreach (Creature_Symbol_On_Map slugcat_symbol in attachedFields.slugcat_symbols)
        {
            slugcat_symbol.Remove_Sprites();
        }

        attachedFields.creature_symbols.Clear();
        attachedFields.slugcat_symbols.Clear();
        all_attached_fields.Remove(map);
    }

    private static void HUD_Map_ctor(On.HUD.Map.orig_ctor orig, Map map, HUD.HUD hud, MapData map_data)
    {
        orig(map, hud, map_data);

        // no arial maps
        if (Option_AerialMap)
        {
            map.STANDARDELEMENTLIST[1] = false;
            map.STANDARDELEMENTLIST[2] = false;
        }

        OwnerType ownerType = map.hud.owner.GetOwnerType();
        if (ownerType != OwnerType.Player && ownerType != OwnerType.SleepScreen && ownerType != OwnerType.DeathScreen && (map.hud.owner is not Overseer overseer || !overseer.SafariOverseer)) return;

        // already initialized;
        if (map.Get_Attached_Fields() != null) return;
        all_attached_fields.Add(map, new());

        if (map.hud.rainWorld.processManager.currentMainLoop is not RainWorldGame game) return;
        if (!game.IsStorySession) return;

        Initialize_Creature_Symbols(map, game);
        Initialize_Slugcat_Symbols(map, game);
    }

    private static void HUD_Map_Draw(On.HUD.Map.orig_Draw orig, Map map, float time_stacker)
    {
        if (map.slatedForDeletion || map.Get_Attached_Fields() is not AttachedFields attached_fields)
        {
            orig(map, time_stacker);
            return;
        }

        Focus_Layer(map, time_stacker);
        orig(map, time_stacker);

        if (map.visible)
        {
            attached_fields.is_map_closed = false;
            Draw_Creature_Symbols(map, time_stacker);
            Draw_Slugcat_Symbols(map, time_stacker);
            return;
        }

        if (attached_fields.is_map_closed) return;
        attached_fields.is_map_closed = true;

        foreach (Creature_Symbol_On_Map creature_symbol in attached_fields.creature_symbols)
        {
            creature_symbol.Is_Visible = false;
        }

        foreach (Creature_Symbol_On_Map slugcat_symbol in attached_fields.slugcat_symbols)
        {
            slugcat_symbol.Is_Visible = false;
        }
    }

    private static void HUD_Map_Update(On.HUD.Map.orig_Update orig, Map map)
    {
        if (map.slatedForDeletion)
        {
            orig(map);
            return;
        }

        if (map.discLoaded && uncovered_rooms.Count > 0)
        {
            Uncover_Room(map, uncovered_rooms[0]);
            uncovered_rooms.RemoveAt(0);
        }

        Skip_Fade(map); // Option_SkipFade
        orig(map);

        // happens only once;
        // map needs to be loaded from disc first;
        // call after orig();
        Initialize_Map_Variables(map);
        Increase_Reveal_Speed(map);
    }

    //
    //
    //

    public sealed class AttachedFields
    {
        public bool is_map_closed;
        public bool is_map_loaded;
        public readonly List<Creature_Symbol_On_Map> creature_symbols = new();
        public readonly List<Creature_Symbol_On_Map> slugcat_symbols = new();
    }
}