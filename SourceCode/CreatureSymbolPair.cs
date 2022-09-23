using UnityEngine;

namespace MapOptions
{
    public class CreatureSymbolPair
    {
        public readonly AbstractCreature abstractCreature;
        public readonly IconSymbol.IconSymbolData iconData;

        public AbstractRoom AbstractRoom => abstractCreature.Room;
        public Color Color
        {
            get; private set;
        }
        public CreatureSymbol? CreatureSymbol
        {
            get; private set;
        }

        public bool IsCreatureDead => abstractCreature.state.dead;
        public bool IsCreatureInDen => abstractCreature.InDen || abstractCreature.Room.offScreenDen;

        public bool? IsVisible
        {
            get
            {
                return CreatureSymbol?.symbolSprite?.isVisible;
            }
            set
            {
                if (value != null && CreatureSymbol?.symbolSprite is FSprite symbolSprite)
                {
                    symbolSprite.isVisible = (bool)value;
                }
            }
        }

        public CreatureSymbolPair(AbstractCreature abstractCreature)
        {
            this.abstractCreature = abstractCreature ?? throw new System.ArgumentNullException("CreatureSymbolPair: AbstractCreature is null.");
            iconData = CreatureSymbol.SymbolDataFromCreature(abstractCreature);
            Color = CreatureSymbol.ColorOfCreature(iconData);
        }

        public void AddSprites()
        {
            if (CreatureSymbol == null)
            {
                Debug.Log("MapOptions: CreatureSymbol is null.");
                return;
            }

            CreatureSymbol.Show(false); // create symbolSprite and add to container
            CreatureSymbol.showFlash = 0.0f;
            CreatureSymbol.lastShowFlash = 0.0f;
        }

        public void CreateCreatureSymbol(FContainer container)
        {
            if (CreatureSymbol != null)
            {
                RemoveSprites();
            }
            CreatureSymbol = new CreatureSymbol(iconData, container);

            if (abstractCreature.realizedCreature is Player player)
            {
                int playerNumber = player.playerState.playerNumber;
                // copied from JollyCoop code
                if (MainMod.IsJollyCoopEnabled && JollyCoop.JollyMod.config.enableColors[playerNumber])
                {
                    Color = JollyCoop.JollyMod.config.playerBodyColors[playerNumber];
                }
                else
                {
                    Color = PlayerGraphics.SlugcatColor(playerNumber);
                }
            }
        }

        public void Draw(AbstractRoom abstractRoom, HUD.Map map, float timeStacker, Vector2 inRoomPos)
        {
            if (CreatureSymbol == null)
            {
                Debug.Log("MapOptions: CreatureSymbol is null.");
                return;
            }

            if (CreatureSymbol.symbolSprite == null)
            {
                AddSprites();
            }

            if (CreatureSymbol.symbolSprite != null) // should always be true
            {
                if (CreatureSymbol.critType == CreatureTemplate.Type.Slugcat)
                {
                    CreatureSymbol.symbolSprite.scale = MapMod.slugcatSymbolScale;
                }
                else
                {
                    CreatureSymbol.symbolSprite.scale = MapMod.creatureSymbolScale;
                }

                if (abstractRoom.layer == map.layer)
                {
                    CreatureSymbol.myColor = Color;
                }
                else
                {
                    CreatureSymbol.myColor = new Color(1f, 1f, 1f);
                }

                CreatureSymbol.symbolSprite.alpha = map.Alpha(abstractRoom.layer, timeStacker, false);
                CreatureSymbol.Draw(timeStacker, map.RoomToMapPos(inRoomPos, abstractRoom.index, timeStacker));
            }
        }

        public bool Equals(CreatureSymbolPair creatureSymbolPair)
        {
            if (CreatureSymbol != null && creatureSymbolPair.CreatureSymbol != null)
            {
                return abstractCreature.Equals(creatureSymbolPair.abstractCreature) && CreatureSymbol.Equals(creatureSymbolPair.CreatureSymbol);
            }

            if (CreatureSymbol == null && creatureSymbolPair.CreatureSymbol == null)
            {
                return abstractCreature.Equals(creatureSymbolPair.abstractCreature);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return abstractCreature.GetHashCode();
        }

        public void RemoveSprites()
        {
            CreatureSymbol?.RemoveSprites(); // remove sprites from container
            CreatureSymbol = null; // "remove" IconSymbol from container
        }
    }
}