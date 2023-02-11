using System.Collections.Generic;
using Menu.Remix.MixedUI;
using UnityEngine;

namespace MapOptions
{
    public class MainModOptions : OptionInterface
    {
        public static MainModOptions instance = new();

        //
        // options
        //

        public static Configurable<bool> aerialMap = instance.config.Bind("aerialMap", defaultValue: true, new ConfigurableInfo("When disabled, the default map shader is used in Chimney Canopy and Sky Islands.", null, "", "Aerial Map"));
        public static Configurable<bool> creatureSymbols = instance.config.Bind("creatureSymbols", defaultValue: true, new ConfigurableInfo("Creature symbols are added to the map. These symbols display what creature types are present in each room.", null, "", "Creature Symbols"));
        public static Configurable<bool> layerFocus = instance.config.Bind("layerFocus", defaultValue: false, new ConfigurableInfo("Only the active layer is displayed on the map.", null, "", "Layer Focus"));
        public static Configurable<bool> skipFade = instance.config.Bind("skipFade", defaultValue: false, new ConfigurableInfo("Pressing the map button shows the map with no delay.", null, "", "Skip Fade In/Out"));

        public static Configurable<bool> slugcatSymbols = instance.config.Bind("slugcatSymbols", defaultValue: true, new ConfigurableInfo("Draws a slugcat sprite on the map instead of a red circle. When Jolly Co-Op is enabled, draws a sprite for each player.", null, "", "Slugcat Symbols"));
        public static Configurable<bool> uncoverRegion = instance.config.Bind("uncoverRegion", defaultValue: false, new ConfigurableInfo("Once loaded into the game the whole region map gets uncovered.\nWARNING: This progress is saved (even without completing a cycle). Turning this option off after saving will *not* remove the gained progress.", null, "", "Uncover Region"));
        public static Configurable<bool> uncoverRoom = instance.config.Bind("uncoverRoom", defaultValue: true, new ConfigurableInfo("When the player enters a room the whole room gets uncovered instead of just the area around slugcat.", null, "", "Uncover Room"));

        public static Configurable<int> zoomSlider = instance.config.Bind("zoomSlider", defaultValue: 10, new ConfigurableInfo("The default value is 100% (10) zoom. Each value (5-15) corresponds to 10*value% (50%-150%) zoom..", new ConfigAcceptableRange<int>(5, 15), "", "Zoom Level (10)"));
        public static Configurable<int> creatureSymbolScale = instance.config.Bind("creatureSymbolScale", defaultValue: 10, new ConfigurableInfo("The default value is 100% (10). Each value (5-20) corresponds to 10*value% (50%-200%).", new ConfigAcceptableRange<int>(5, 20), "", "Creature Symbol Size (10)"));
        public static Configurable<int> slugcatSymbolScale = instance.config.Bind("slugcatSymbolScale", defaultValue: 10, new ConfigurableInfo("The default value is 100% (10). Each value (5-20) corresponds to 10*value% (50%-200%).", new ConfigAcceptableRange<int>(5, 20), "", "Slugcat Symbol Size (10)"));
        public static Configurable<int> revealSpeedMultiplier = instance.config.Bind("revealSpeedMultiplier", defaultValue: 1, new ConfigurableInfo("The default value is one. For a given value X the map is revealed X-times as fast.\nIf the maximum value is selected then opening the map displays known areas instantly instead of revealing them gradually.", new ConfigAcceptableRange<int>(1, 10), "", "Reveal Speed Multiplier (1)"));

        //
        // parameters
        //

        private readonly float fontHeight = 20f;
        private readonly float spacing = 20f;
        private readonly int numberOfCheckboxes = 3;
        private readonly float checkBoxSize = 24f;
        private float CheckBoxWithSpacing => checkBoxSize + 0.25f * spacing;

        //
        // variables
        //

        private Vector2 marginX = new();
        private Vector2 pos = new();
        private readonly List<OpLabel> textLabels = new();
        private readonly List<float> boxEndPositions = new();

        private readonly List<Configurable<bool>> checkBoxConfigurables = new();
        private readonly List<OpLabel> checkBoxesTextLabels = new();

        private readonly List<Configurable<int>> sliderConfigurables = new();
        private readonly List<string> sliderMainTextLabels = new();
        private readonly List<OpLabel> sliderTextLabelsLeft = new();
        private readonly List<OpLabel> sliderTextLabelsRight = new();

        //
        // main
        //

        public MainModOptions()
        {
            // ambiguity error // why? TODO
            // OnConfigChanged += MainModOptions_OnConfigChanged;
        }

        //
        // public
        //

        public override void Initialize()
        {
            base.Initialize();
            Tabs = new OpTab[1];

            int tabIndex = 0;
            Tabs[tabIndex] = new OpTab(instance, "Options");
            InitializeMarginAndPos();

            // Title
            AddNewLine();
            AddTextLabel("MapOptions Mod", bigText: true);
            DrawTextLabels(ref Tabs[tabIndex]);

            // Subtitle
            AddNewLine(0.5f);
            AddTextLabel("Version " + MainMod.version, FLabelAlignment.Left);
            AddTextLabel("by " + MainMod.author, FLabelAlignment.Right);
            DrawTextLabels(ref Tabs[tabIndex]);

            // Content //
            AddNewLine();
            AddBox();

            AddSlider(zoomSlider, (string)zoomSlider.info.Tags[0], "50%", "150%");
            DrawSliders(ref Tabs[tabIndex]);

            AddNewLine();

            AddCheckBox(aerialMap, (string)aerialMap.info.Tags[0]);
            AddCheckBox(creatureSymbols, (string)creatureSymbols.info.Tags[0]);
            AddCheckBox(layerFocus, (string)layerFocus.info.Tags[0]);
            AddCheckBox(skipFade, (string)skipFade.info.Tags[0]);
            AddCheckBox(slugcatSymbols, (string)slugcatSymbols.info.Tags[0]);
            AddCheckBox(uncoverRegion, (string)uncoverRegion.info.Tags[0]);
            AddCheckBox(uncoverRoom, (string)uncoverRoom.info.Tags[0]);
            DrawCheckBoxes(ref Tabs[tabIndex]);

            AddNewLine();

            AddSlider(creatureSymbolScale, (string)creatureSymbolScale.info.Tags[0], "50%", "200%");
            AddSlider(slugcatSymbolScale, (string)slugcatSymbolScale.info.Tags[0], "50%", "200%");
            AddSlider(revealSpeedMultiplier, (string)revealSpeedMultiplier.info.Tags[0], "1", "10");
            DrawSliders(ref Tabs[tabIndex]);

            DrawBox(ref Tabs[tabIndex]);
        }

        public void MainModOptions_OnConfigChanged()
        {
            MapMod.creatureSymbolScale = creatureSymbolScale.Value / 10f;
            MapMod.mapScale = 10f / zoomSlider.Value;
            MapMod.slugcatSymbolScale = slugcatSymbolScale.Value / 10f;
            MapMod.revealSpeedMultiplier = revealSpeedMultiplier.Value;

            Debug.Log("MapOptions: creatureSymbolScale " + MapMod.creatureSymbolScale);
            Debug.Log("MapOptions: mapScale " + MapMod.mapScale);
            Debug.Log("MapOptions: slugcatSymbolScale " + MapMod.slugcatSymbolScale);

            Debug.Log("MapOptions: revealSpeedMultiplier " + MapMod.revealSpeedMultiplier);
            Debug.Log("MapOptions: CanInstantReveal " + MapMod.CanInstantReveal);

            Debug.Log("MapOptions: Option_CreatureSymbols " + MainMod.Option_CreatureSymbols);
            Debug.Log("MapOptions: Option_SlugcatSymbols " + MainMod.Option_SlugcatSymbols);
            Debug.Log("MapOptions: Option_UncoverRegion " + MainMod.Option_UncoverRegion);

            Debug.Log("MapOptions: Option_UncoverRoom " + MainMod.Option_UncoverRoom);
            Debug.Log("MapOptions: Option_LayerFocus " + MainMod.Option_LayerFocus);
            Debug.Log("MapOptions: Option_SkipFade " + MainMod.Option_SkipFade);
        }

        // ----------------- //
        // private functions //
        // ----------------- //

        private void InitializeMarginAndPos()
        {
            marginX = new Vector2(50f, 550f);
            pos = new Vector2(50f, 600f);
        }

        private void AddNewLine(float spacingModifier = 1f)
        {
            pos.x = marginX.x; // left margin
            pos.y -= spacingModifier * spacing;
        }

        private void AddBox()
        {
            marginX += new Vector2(spacing, -spacing);
            boxEndPositions.Add(pos.y); // end position > start position
            AddNewLine();
        }

        private void DrawBox(ref OpTab tab)
        {
            marginX += new Vector2(-spacing, spacing);
            AddNewLine();

            float boxWidth = marginX.y - marginX.x;
            int lastIndex = boxEndPositions.Count - 1;

            tab.AddItems(new OpRect(pos, new Vector2(boxWidth, boxEndPositions[lastIndex] - pos.y)));
            boxEndPositions.RemoveAt(lastIndex);
        }

        private void AddCheckBox(Configurable<bool> configurable, string text)
        {
            checkBoxConfigurables.Add(configurable);
            checkBoxesTextLabels.Add(new OpLabel(new Vector2(), new Vector2(), text, FLabelAlignment.Left));
        }

        private void DrawCheckBoxes(ref OpTab tab) // changes pos.y but not pos.x
        {
            if (checkBoxConfigurables.Count != checkBoxesTextLabels.Count) return;

            float width = marginX.y - marginX.x;
            float elementWidth = (width - (numberOfCheckboxes - 1) * 0.5f * spacing) / numberOfCheckboxes;
            pos.y -= checkBoxSize;
            float _posX = pos.x;

            for (int checkBoxIndex = 0; checkBoxIndex < checkBoxConfigurables.Count; ++checkBoxIndex)
            {
                Configurable<bool> configurable = checkBoxConfigurables[checkBoxIndex];
                OpCheckBox checkBox = new(configurable, new Vector2(_posX, pos.y))
                {
                    description = configurable.info?.description ?? ""
                };
                tab.AddItems(checkBox);
                _posX += CheckBoxWithSpacing;

                OpLabel checkBoxLabel = checkBoxesTextLabels[checkBoxIndex];
                checkBoxLabel.pos = new Vector2(_posX, pos.y + 2f);
                checkBoxLabel.size = new Vector2(elementWidth - CheckBoxWithSpacing, fontHeight);
                tab.AddItems(checkBoxLabel);

                if (checkBoxIndex < checkBoxConfigurables.Count - 1)
                {
                    if ((checkBoxIndex + 1) % numberOfCheckboxes == 0)
                    {
                        AddNewLine();
                        pos.y -= checkBoxSize;
                        _posX = pos.x;
                    }
                    else
                    {
                        _posX += elementWidth - CheckBoxWithSpacing + 0.5f * spacing;
                    }
                }
            }

            checkBoxConfigurables.Clear();
            checkBoxesTextLabels.Clear();
        }

        private void AddSlider(Configurable<int> configurable, string text, string sliderTextLeft = "", string sliderTextRight = "")
        {
            sliderConfigurables.Add(configurable);
            sliderMainTextLabels.Add(text);
            sliderTextLabelsLeft.Add(new OpLabel(new Vector2(), new Vector2(), sliderTextLeft, alignment: FLabelAlignment.Right)); // set pos and size when drawing
            sliderTextLabelsRight.Add(new OpLabel(new Vector2(), new Vector2(), sliderTextRight, alignment: FLabelAlignment.Left));
        }

        private void DrawSliders(ref OpTab tab)
        {
            if (sliderConfigurables.Count != sliderMainTextLabels.Count) return;
            if (sliderConfigurables.Count != sliderTextLabelsLeft.Count) return;
            if (sliderConfigurables.Count != sliderTextLabelsRight.Count) return;

            float width = marginX.y - marginX.x;
            float sliderCenter = marginX.x + 0.5f * width;
            float sliderLabelSizeX = 0.2f * width;
            float sliderSizeX = width - 2f * sliderLabelSizeX - spacing;

            for (int sliderIndex = 0; sliderIndex < sliderConfigurables.Count; ++sliderIndex)
            {
                AddNewLine(2f);

                OpLabel opLabel = sliderTextLabelsLeft[sliderIndex];
                opLabel.pos = new Vector2(marginX.x, pos.y + 5f);
                opLabel.size = new Vector2(sliderLabelSizeX, fontHeight);
                tab.AddItems(opLabel);

                Configurable<int> configurable = sliderConfigurables[sliderIndex];
                OpSlider slider = new(configurable, new Vector2(sliderCenter - 0.5f * sliderSizeX, pos.y), (int)sliderSizeX)
                {
                    size = new Vector2(sliderSizeX, fontHeight),
                    description = configurable.info?.description ?? ""
                };
                tab.AddItems(slider);

                opLabel = sliderTextLabelsRight[sliderIndex];
                opLabel.pos = new Vector2(sliderCenter + 0.5f * sliderSizeX + 0.5f * spacing, pos.y + 5f);
                opLabel.size = new Vector2(sliderLabelSizeX, fontHeight);
                tab.AddItems(opLabel);

                AddTextLabel(sliderMainTextLabels[sliderIndex]);
                DrawTextLabels(ref tab);

                if (sliderIndex < sliderConfigurables.Count - 1)
                {
                    AddNewLine();
                }
            }

            sliderConfigurables.Clear();
            sliderMainTextLabels.Clear();
            sliderTextLabelsLeft.Clear();
            sliderTextLabelsRight.Clear();
        }

        private void AddTextLabel(string text, FLabelAlignment alignment = FLabelAlignment.Center, bool bigText = false)
        {
            float textHeight = (bigText ? 2f : 1f) * fontHeight;
            if (textLabels.Count == 0)
            {
                pos.y -= textHeight;
            }

            OpLabel textLabel = new(new Vector2(), new Vector2(20f, textHeight), text, alignment, bigText) // minimal size.x = 20f
            {
                autoWrap = true
            };
            textLabels.Add(textLabel);
        }

        //private void DrawTextLabels(ref OpScrollBox scrollBox)
        //{
        //    if (textLabels.Count == 0)
        //    {
        //        return;
        //    }

        //    float width = (marginX.y - marginX.x) / textLabels.Count;
        //    foreach (OpLabel textLabel in textLabels)
        //    {
        //        textLabel.pos = pos;
        //        textLabel.size += new Vector2(width - 20f, 0.0f);
        //        scrollBox.AddItems(new UIelement[] { textLabel });
        //        pos.x += width;
        //    }

        //    pos.x = marginX.x;
        //    textLabels.Clear();
        //}

        private void DrawTextLabels(ref OpTab tab)
        {
            if (textLabels.Count == 0)
            {
                return;
            }

            float width = (marginX.y - marginX.x) / textLabels.Count;
            foreach (OpLabel textLabel in textLabels)
            {
                textLabel.pos = pos;
                textLabel.size += new Vector2(width - 20f, 0.0f);
                tab.AddItems(textLabel);
                pos.x += width;
            }

            pos.x = marginX.x;
            textLabels.Clear();
        }
    }
}