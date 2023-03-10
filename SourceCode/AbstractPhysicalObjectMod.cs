using System.Collections.Generic;
using HUD;

using static AbstractPhysicalObject;
using static HUD.Map;
using static MapOptions.MainMod;
using static MapOptions.MapMod;

namespace MapOptions;

internal static class AbstractPhysicalObjectMod
{
    //
    // variables
    //

    private static bool is_enabled = false;
    internal static List<AbstractPhysicalObject> items_that_need_trackers = new();

    //
    //
    //

    internal static void OnToggle()
    {
        is_enabled = !is_enabled;
        if (Option_ItemTracker)
        {
            if (is_enabled)
            {
                On.AbstractPhysicalObject.ctor += AbstractPhysicalObject_Ctor;
            }
            else
            {
                On.AbstractPhysicalObject.ctor -= AbstractPhysicalObject_Ctor;
            }
        }
    }

    //
    // public
    //

    public static void Add_Key_Item_Tracker()
    {
        // this function and most other hooks for Option_ItemTracker
        // don't add much value;
        // the only thing is that this way I can have item markers 
        // without trackers (i.e. things don't need to be grabbed);
        // I feel like this is not that important;

        if (items_that_need_trackers.Count == 0) return;

        int index = items_that_need_trackers.Count - 1;
        AbstractPhysicalObject item = items_that_need_trackers[index];

        if (item.world?.game is not RainWorldGame game || game.session is not StoryGameSession || !UsesAPersistantTracker(item))
        {
            items_that_need_trackers.RemoveAt(index);
            return;
        }

        // wait until the region is loaded;
        // otherwise, you might freeze the game;
        if (game.overWorld.worldLoader != null) return;

        // AddNewPersistentTracker() assumes that the map for camera_0 is initialized;
        if (game.cameras.Length == 0 || game.cameras[0].hud?.map is not Map map) return;

        bool is_in_stomach = false;
        foreach (AbstractCreature abstract_player in game.Players)
        {
            // check for hunter start;
            // SwallowObject() removes the tracker and item marker but is not called in hunter start;
            // Regurgitate() adds the tracker and item marker;
            // skip here; otherwise, the pearl might get duplicated (when adding a tracker);
            // or the position will not get updated (when adding an item marker);
            if (abstract_player.realizedCreature is not Player player) continue;
            if (player.objectInStomach != item) continue;
            is_in_stomach = true;
            break;
        }

        if (is_in_stomach)
        {
            items_that_need_trackers.RemoveAt(index);
            return;
        }

        if (Has_Item_Marker(map, item))
        {
            items_that_need_trackers.RemoveAt(index);
            return;
        }

        // only add item markers;
        // otherwise, tracked items might get shoved around
        // before you even reach them;

        // story_session.AddNewPersistentTracker(item);

        // this does not work either;
        // trackers and markers are linked;
        // the marker shows the potential or desired spawn
        // before the item spawns;
        // and they assume that the index is the same;
        // creating item markers without trackers could mess that up
        // when the trackers get created later;
        //
        // not anymore, I removed the link;
        // the downside is still that they only show after the
        // object spawned; there is no such thing as a desired spawn location now;

        ItemMarker item_marker = new(map, item.Room.index, item.pos.Tile.ToVector2(), item);
        map.itemMarkers.Add(item_marker);
        map.mapObjects.Add(item_marker);

        items_that_need_trackers.RemoveAt(index);
    }



    //
    // private
    //

    private static void AbstractPhysicalObject_Ctor(On.AbstractPhysicalObject.orig_ctor orig, AbstractPhysicalObject abstract_physical_object, World? world, AbstractObjectType type, PhysicalObject realized_object, WorldCoordinate coordinate, EntityID id)
    {
        orig(abstract_physical_object, world, type, realized_object, coordinate, id);

        if (world?.game.session is not StoryGameSession) return;

        // this might not be accurate since abstract_physical_object might not be fully initialized;
        // often this is base() and some stuff is added after this function returns;
        // therefore, I can't call AddNewPersistentTracker() either yet;
        // if (!UsesAPersistantTracker(abstract_physical_object)) return;

        items_that_need_trackers.Add(abstract_physical_object);
    }
}