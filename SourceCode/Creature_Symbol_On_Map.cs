using UnityEngine;

using static CreatureSymbol;
using static MapOptions.MainMod;
using static MapOptions.MapMod;

namespace MapOptions;

public class Creature_Symbol_On_Map
{
    //
    // parameters
    //

    public readonly AbstractCreature abstract_creature;
    public readonly CreatureSymbol creature_symbol;
    public readonly Color default_color;

    //
    // variables
    //

    public AbstractRoom Abstract_Room => abstract_creature.Room;

    public bool Is_Creature_Dead => abstract_creature.state.dead;
    public bool Is_Creature_In_Den => abstract_creature.InDen || abstract_creature.Room.offScreenDen;
    public bool? Is_Visible
    {
        get => Symbol_Sprite?.isVisible;
        set
        {
            if (creature_symbol.shadowSprite1 != null)
            {
                creature_symbol.shadowSprite1.isVisible = value ?? false;
            }

            if (creature_symbol.shadowSprite2 != null)
            {
                creature_symbol.shadowSprite2.isVisible = value ?? false;
            }

            if (Symbol_Sprite == null) return;
            Symbol_Sprite.isVisible = value ?? false;
        }
    }

    public FSprite? Symbol_Sprite => creature_symbol.symbolSprite;

    //
    // main
    //

    public Creature_Symbol_On_Map(AbstractCreature abstract_creature, FContainer container)
    {
        this.abstract_creature = abstract_creature;
        creature_symbol = new(SymbolDataFromCreature(abstract_creature), container);

        if (abstract_creature.realizedCreature is Player player)
        {
            default_color = PlayerGraphics.SlugcatColor(player.playerState.slugcatCharacter);
            creature_symbol.myColor = default_color;
        }
        else
        {
            default_color = creature_symbol.myColor;
        }
        Add_Sprites();
    }

    //
    // public
    //

    public void Add_Sprites()
    {
        if (Symbol_Sprite != null) return;

        // creates symbolSprite and adds it to the container;
        // creature_symbol.Show(false);
        creature_symbol.Show(showShadowSprites: Option_ShadowSprites);

        if (Symbol_Sprite == null) return;

        creature_symbol.showFlash = 0.0f;
        creature_symbol.lastShowFlash = 0.0f;
        Symbol_Sprite.scale = creature_symbol.critType == CreatureTemplate.Type.Slugcat ? slugcat_symbols_scale : creature_symbols_scale;

        // otherwise they are show in the bottom left corner
        Symbol_Sprite.isVisible = false;
    }

    public void Change_Alpha(HUD.Map map, float time_stacker)
    {
        float alpha = map.Alpha(Abstract_Room.layer, time_stacker, false);

        if (creature_symbol.shadowSprite1 != null)
        {
            creature_symbol.shadowSprite1.alpha = alpha;
        }

        if (creature_symbol.shadowSprite2 != null)
        {
            creature_symbol.shadowSprite2.alpha = alpha;
        }

        if (Symbol_Sprite == null) return;
        Symbol_Sprite.alpha = alpha;

    }

    public void Draw(HUD.Map map, float time_stacker, Vector2 in_room_position)
    {
        if (Symbol_Sprite == null) return;

        creature_symbol.myColor = Abstract_Room.layer == map.layer ? default_color : new Color(1f, 1f, 1f);
        Change_Alpha(map, time_stacker);
        creature_symbol.Draw(time_stacker, map.RoomToMapPos(in_room_position, Abstract_Room.index, time_stacker));
    }

    public void Remove_Sprites() => creature_symbol.RemoveSprites();
}