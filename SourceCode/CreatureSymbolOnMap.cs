using UnityEngine;

namespace MapOptions
{
    public class CreatureSymbolOnMap : CreatureSymbol
    {
        //
        // variables
        //

        public readonly AbstractCreature abstractCreature;

        public Color DefaultColor
        {
            get; private set;
        }

        //
        //
        //

        public AbstractRoom AbstractRoom => abstractCreature.Room;
        public bool IsCreatureDead => abstractCreature.state.dead;
        public bool IsCreatureInDen => abstractCreature.InDen || abstractCreature.Room.offScreenDen;

        public bool? IsVisible
        {
            get => symbolSprite?.isVisible;
            set
            {
                if (symbolSprite != null)
                {
                    symbolSprite.isVisible = value ?? false;
                }
            }
        }

        //
        //
        //

        public CreatureSymbolOnMap(AbstractCreature abstractCreature, FContainer container) : base(SymbolDataFromCreature(abstractCreature ?? throw new System.ArgumentNullException("CreatureSymbolOnMap: AbstractCreature is null.")), container)
        {
            this.abstractCreature = abstractCreature ?? throw new System.ArgumentNullException("CreatureSymbolOnMap: AbstractCreature is null.");
            if (abstractCreature.realizedCreature is Player player)
            {
                int playerNumber = player.playerState.playerNumber;
                // copied from JollyCoop code
                if (MainMod.IsJollyCoopEnabled && JollyCoop.JollyMod.config.enableColors[playerNumber])
                {
                    DefaultColor = JollyCoop.JollyMod.config.playerBodyColors[playerNumber];
                }
                else
                {
                    DefaultColor = PlayerGraphics.SlugcatColor(playerNumber);
                }
                myColor = DefaultColor;
            }
            else
            {
                DefaultColor = myColor;
            }
            AddSprites();
        }

        public void AddSprites()
        {
            if (symbolSprite != null) return;

            base.Show(false); // create symbolSprite and add to container
            if (symbolSprite == null) return;

            showFlash = 0.0f;
            lastShowFlash = 0.0f;
            symbolSprite.scale = critType == CreatureTemplate.Type.Slugcat ? MapMod.slugcatSymbolScale : MapMod.creatureSymbolScale;
            IsVisible = false; // otherwise they are show in the bottom left corner
        }

        public void Draw(HUD.Map map, float timeStacker, Vector2 inRoomPos)
        {
            if (symbolSprite == null) return;

            AbstractRoom abstractRoom = AbstractRoom;
            myColor = abstractRoom.layer == map.layer ? DefaultColor : new Color(1f, 1f, 1f);
            symbolSprite.alpha = map.Alpha(abstractRoom.layer, timeStacker, false);
            Draw(timeStacker, map.RoomToMapPos(inRoomPos, abstractRoom.index, timeStacker));
        }
    }
}