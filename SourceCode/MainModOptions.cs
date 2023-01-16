using System.Collections.Generic;
using OptionalUI;
using RWCustom;
using UnityEngine;

namespace MapOptions
{
    public class MainModOptions : OptionInterface
    {
        private Vector2 marginX = new();
        private Vector2 pos = new();
        private readonly float spacing = 20f;

        private readonly List<float> boxEndPositions = new();

        private readonly int numberOfCheckboxes = 3;
        private readonly float checkBoxSize = 24f;
        private readonly List<OpCheckBox> checkBoxes = new();
        private readonly List<OpLabel> checkBoxesTextLabels = new();

        //private readonly List<OpComboBox> comboBoxes = new List<OpComboBox>();
        //private readonly List<OpLabel> comboBoxesTextLabels = new List<OpLabel>();

        private readonly List<string> sliderKeys = new();
        private readonly List<IntVector2> sliderRanges = new();
        private readonly List<int> sliderDefaultValues = new();
        private readonly List<string> sliderDescriptions = new();
        private readonly List<string> sliderMainTextLabels = new();
        private readonly List<OpLabel> sliderTextLabelsLeft = new();
        private readonly List<OpLabel> sliderTextLabelsRight = new();

        private readonly float fontHeight = 20f;
        private readonly List<OpLabel> textLabels = new();

        private float CheckBoxWithSpacing => checkBoxSize + 0.25f * spacing;

        public MainModOptions() : base(plugin: MainMod.instance) { }

        public override void Initialize()
        {
            base.Initialize();
            Tabs = new OpTab[1];

            int tabIndex = 0;
            Tabs[tabIndex] = new OpTab("Options");
            InitializeMarginAndPos();

            // Title
            AddNewLine();
            AddTextLabel("MapOptions Mod", bigText: true);
            DrawTextLabels(ref Tabs[tabIndex]);

            // Subtitle
            AddNewLine(0.5f);
            AddTextLabel("Version " + MainMod.instance?.Info.Metadata.Version, FLabelAlignment.Left);
            AddTextLabel("by " + MainMod.instance?.author, FLabelAlignment.Right);
            DrawTextLabels(ref Tabs[tabIndex]);

            // Content //
            AddNewLine();
            AddBox();

            AddSlider("zoomSlider", "Zoom Level (10)", "The default value is 100% (10) zoom. Each value (5-15) corresponds to 10*value% (50%-150%) zoom.", new IntVector2(5, 15), defaultValue: 10, "50%", "150%");
            DrawSliders(ref Tabs[tabIndex]);

            AddNewLine();

            AddCheckBox("displayCreatureSymbols", "Creature Symbols", "Creature symbols are added to the map. These symbols display what creature types are present in each room.", defaultBool: true);
            //AddCheckBox("instantReveal", "Instant Reveal", "Opening the map displays known areas instantly instead of revealing them gradually.", defaultBool: false);
            AddCheckBox("showOnlyActiveLayer", "Layer Focus", "Only the active layer is displayed on the map.", defaultBool: false);
            AddCheckBox("skipFade", "Skip Fade In/Out", "Pressing the map button shows the map with no delay.", defaultBool: false);
            AddCheckBox("slugcatSymbols", "Slugcat Symbols", "Draws a slugcat sprite on the map instead of a red circle. When Jolly Co-Op Mod is enabled, draws a sprite for each player.", defaultBool: true);
            AddCheckBox("uncoverRegion", "Uncover Region", "Once loaded into the game the whole region map gets uncovered.\nWARNING: This progress is saved (even without completing a cycle). Turning this option off after saving will *not* remove the gained progress.", defaultBool: false);
            AddCheckBox("uncoverRoom", "Uncover Room", "When the player enters a room the whole room gets uncovered instead of just the area around slugcat.", defaultBool: true);
            DrawCheckBoxes(ref Tabs[tabIndex]);

            AddNewLine();

            AddSlider("creatureSymbolScale", "Creature Symbol Size (10)", "The default value is 100% (10). Each value (5-20) corresponds to 10*value% (50%-200%).", new IntVector2(5, 20), defaultValue: 10, "50%", "200%");
            AddSlider("slugcatSymbolScale", "Slugcat Symbol Size (10)", "The default value is 100% (10). Each value (5-20) corresponds to 10*value% (50%-200%).", new IntVector2(5, 20), defaultValue: 10, "50%", "200%");
            AddSlider("revealSpeedMultiplier", "Reveal Speed Multiplier (1)", "The default value is one. For a given value X the map is revealed X-times as fast.\nIf the maximum value is selected then opening the map displays known areas instantly instead of revealing them gradually.", new IntVector2(1, 10), defaultValue: 1, "1", "10");
            DrawSliders(ref Tabs[tabIndex]);

            DrawBox(ref Tabs[tabIndex]);
        }
        public override void Update(float dt)
        {
            base.Update(dt);
        }
        public override void ConfigOnChange()
        {
            base.ConfigOnChange();

            MapMod.creatureSymbolScale = float.Parse(config["creatureSymbolScale"]) / 10f;
            MapMod.mapScale = 10f / float.Parse(config["zoomSlider"]);
            MapMod.slugcatSymbolScale = float.Parse(config["slugcatSymbolScale"]) / 10f;
            MapMod.revealSpeedMultiplier = int.Parse(config["revealSpeedMultiplier"]);

            MainMod.creatureSymbolsOption = bool.Parse(config["displayCreatureSymbols"]);
            MainMod.slugcatSymbolsOption = bool.Parse(config["slugcatSymbols"]);

            // it can help to have both options turned on // since one pixel check is random and the others might prevent uncover because of pipes leading into a room
            MainMod.uncoverRegionOption = bool.Parse(config["uncoverRegion"]);
            MainMod.uncoverRoomOption = bool.Parse(config["uncoverRoom"]);

            MainMod.onlyActiveLayerOption = bool.Parse(config["showOnlyActiveLayer"]);
            MainMod.skipFadeOption = bool.Parse(config["skipFade"]);

            Debug.Log("MapOptions: creatureSymbolScale " + MapMod.creatureSymbolScale);
            Debug.Log("MapOptions: mapScale " + MapMod.mapScale);
            Debug.Log("MapOptions: slugcatSymbolScale " + MapMod.slugcatSymbolScale);

            Debug.Log("MapOptions: revealSpeedMultiplier " + MapMod.revealSpeedMultiplier);
            Debug.Log("MapOptions: CanInstantReveal " + MapMod.CanInstantReveal);

            Debug.Log("MapOptions: creatureSymbolsOption " + MainMod.creatureSymbolsOption);
            Debug.Log("MapOptions: slugcatSymbolsOption " + MainMod.slugcatSymbolsOption);
            Debug.Log("MapOptions: uncoverRegionOption " + MainMod.uncoverRegionOption);
            Debug.Log("MapOptions: uncoverRoomOption " + MainMod.uncoverRoomOption);

            Debug.Log("MapOptions: onlyActiveLayerOption " + MainMod.onlyActiveLayerOption);
            Debug.Log("MapOptions: skipFadeOption " + MainMod.skipFadeOption);
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

        //private OpScrollBox AddScrollBox(ref OpTab tab, float spacingModifier = 1f)
        //{
        //    float boxWidth = marginX.y - marginX.x;
        //    float marginY = spacingModifier * spacing;
        //    boxEndPositions.Add(pos.y);

        //    OpScrollBox scrollBox = new OpScrollBox(new Vector2(marginX.x, marginY), new Vector2(boxWidth, Math.Max(pos.y - marginY, spacing)), boxWidth);
        //    tab.AddItems(scrollBox);

        //    marginX -= new Vector2(30f, 70f);
        //    AddNewLine(0.5f);
        //    return scrollBox;
        //}

        //private void DrawScrollBox(ref OpScrollBox scrollBox)
        //{
        //    AddNewLine(1.5f);
        //    int lastIndex = boxEndPositions.Count - 1;

        //    scrollBox.SetContentSize(boxEndPositions[lastIndex] - pos.y);
        //    scrollBox.ScrollToTop();
        //    boxEndPositions.RemoveAt(lastIndex);
        //}

        private void AddCheckBox(string key, string text, string description, bool? defaultBool = null)
        {
            OpCheckBox opCheckBox = new(new Vector2(), key, defaultBool: defaultBool ?? false)
            {
                description = description
            };

            checkBoxes.Add(opCheckBox);
            checkBoxesTextLabels.Add(new OpLabel(new Vector2(), new Vector2(), text, FLabelAlignment.Left));
        }

        private void DrawCheckBoxes(ref OpTab tab) // changes pos.y but not pos.x
        {
            if (checkBoxes.Count != checkBoxesTextLabels.Count)
            {
                return;
            }

            float width = marginX.y - marginX.x;
            float elementWidth = (width - (numberOfCheckboxes - 1) * 0.5f * spacing) / numberOfCheckboxes;
            pos.y -= checkBoxSize;
            float _posX = pos.x;

            for (int index = 0; index < checkBoxes.Count; ++index)
            {
                OpCheckBox checkBox = checkBoxes[index];
                checkBox.pos = new Vector2(_posX, pos.y);
                tab.AddItems(checkBox);
                _posX += CheckBoxWithSpacing;

                OpLabel checkBoxLabel = checkBoxesTextLabels[index];
                checkBoxLabel.pos = new Vector2(_posX, pos.y + 2f);
                checkBoxLabel.size = new Vector2(elementWidth - CheckBoxWithSpacing, fontHeight);
                tab.AddItems(checkBoxLabel);

                if (index < checkBoxes.Count - 1)
                {
                    if ((index + 1) % numberOfCheckboxes == 0)
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

            checkBoxes.Clear();
            checkBoxesTextLabels.Clear();
        }

        //private void AddComboBox(List<ListItem> list, string key, string text, string description, string defaultName = "", bool allowEmpty = false)
        //{
        //    OpLabel opLabel = new OpLabel(new Vector2(), new Vector2(0.0f, fontHeight), text, FLabelAlignment.Center, false);
        //    comboBoxesTextLabels.Add(opLabel);

        //    OpComboBox opComboBox = new OpComboBox(new Vector2(), 200f, key, list, defaultName)
        //    {
        //        allowEmpty = allowEmpty,
        //        description = description
        //    };
        //    comboBoxes.Add(opComboBox);
        //}

        //private void DrawComboBoxes(ref OpTab tab)
        //{
        //    if (comboBoxes.Count == 0 || comboBoxes.Count != comboBoxesTextLabels.Count)
        //    {
        //        return;
        //    }

        //    float offsetX = (marginX.y - marginX.x) * 0.1f;
        //    float width = (marginX.y - marginX.x) * 0.4f;

        //    for (int comboBoxIndex = 0; comboBoxIndex < comboBoxes.Count; ++comboBoxIndex)
        //    {
        //        AddNewLine(1.25f);
        //        pos.x += offsetX;

        //        OpLabel opLabel = comboBoxesTextLabels[comboBoxIndex];
        //        opLabel.pos = pos;
        //        opLabel.size += new Vector2(width, 2f); // size.y is already set
        //        pos.x += width;

        //        OpComboBox comboBox = comboBoxes[comboBoxIndex];
        //        OpComboBox newComboBox = new OpComboBox(pos, width, comboBox.key, comboBox.GetItemList().ToList(), defaultName: comboBox.defaultValue)
        //        {
        //            allowEmpty = comboBox.allowEmpty,
        //            description = comboBox.description,
        //        };
        //        tab.AddItems(opLabel, newComboBox);

        //        if (comboBoxIndex < checkBoxes.Count - 1)
        //        {
        //            AddNewLine();
        //            pos.x = marginX.x;
        //        }
        //    }

        //    comboBoxesTextLabels.Clear();
        //    comboBoxes.Clear();
        //}

        private void AddSlider(string key, string text, string description, IntVector2 range, int defaultValue, string? sliderTextLeft = null, string? sliderTextRight = null)
        {
            sliderTextLeft ??= range.x.ToString();
            sliderTextRight ??= range.y.ToString();

            sliderMainTextLabels.Add(text);
            sliderTextLabelsLeft.Add(new OpLabel(new Vector2(), new Vector2(), sliderTextLeft, alignment: FLabelAlignment.Right)); // set pos and size when drawing
            sliderTextLabelsRight.Add(new OpLabel(new Vector2(), new Vector2(), sliderTextRight, alignment: FLabelAlignment.Left));

            sliderKeys.Add(key);
            sliderRanges.Add(range);
            sliderDefaultValues.Add(defaultValue);
            sliderDescriptions.Add(description);
        }

        //private void DrawSliders(ref OpScrollBox scrollBox)
        //{
        //    if (sliderKeys.Count != sliderRanges.Count || sliderKeys.Count != sliderDefaultValues.Count || sliderKeys.Count != sliderDescriptions.Count || sliderKeys.Count != sliderMainTextLabels.Count || sliderKeys.Count != sliderTextLabelsLeft.Count || sliderKeys.Count != sliderTextLabelsRight.Count)
        //    {
        //        return;
        //    }

        //    float width = marginX.y - marginX.x;
        //    float sliderCenter = marginX.x + 0.5f * width;
        //    float sliderLabelSizeX = 0.2f * width;
        //    float sliderSizeX = width - 2f * sliderLabelSizeX - spacing;

        //    for (int sliderIndex = 0; sliderIndex < sliderKeys.Count; ++sliderIndex)
        //    {
        //        AddNewLine(2f);

        //        OpLabel opLabel = sliderTextLabelsLeft[sliderIndex];
        //        opLabel.pos = new Vector2(marginX.x, pos.y + 5f);
        //        opLabel.size = new Vector2(sliderLabelSizeX, fontHeight);
        //        scrollBox.AddItems(opLabel);

        //        OpSlider slider = new OpSlider(new Vector2(sliderCenter - 0.5f * sliderSizeX, pos.y), sliderKeys[sliderIndex], sliderRanges[sliderIndex], length: (int)sliderSizeX, defaultValue: sliderDefaultValues[sliderIndex])
        //        {
        //            size = new Vector2(sliderSizeX, fontHeight),
        //            description = sliderDescriptions[sliderIndex]
        //        };
        //        scrollBox.AddItems(slider);

        //        opLabel = sliderTextLabelsRight[sliderIndex];
        //        opLabel.pos = new Vector2(sliderCenter + 0.5f * sliderSizeX + 0.5f * spacing, pos.y + 5f);
        //        opLabel.size = new Vector2(sliderLabelSizeX, fontHeight);
        //        scrollBox.AddItems(opLabel);

        //        AddTextLabel(sliderMainTextLabels[sliderIndex]);
        //        DrawTextLabels(ref scrollBox);

        //        if (sliderIndex < sliderKeys.Count - 1)
        //        {
        //            AddNewLine();
        //        }
        //    }

        //    sliderKeys.Clear();
        //    sliderRanges.Clear();
        //    sliderDefaultValues.Clear();
        //    sliderDescriptions.Clear();

        //    sliderMainTextLabels.Clear();
        //    sliderTextLabelsLeft.Clear();
        //    sliderTextLabelsRight.Clear();
        //}

        private void DrawSliders(ref OpTab tab)
        {
            if (sliderKeys.Count != sliderRanges.Count || sliderKeys.Count != sliderDefaultValues.Count || sliderKeys.Count != sliderDescriptions.Count || sliderKeys.Count != sliderMainTextLabels.Count || sliderKeys.Count != sliderTextLabelsLeft.Count || sliderKeys.Count != sliderTextLabelsRight.Count)
            {
                return;
            }

            float width = marginX.y - marginX.x;
            float sliderCenter = marginX.x + 0.5f * width;
            float sliderLabelSizeX = 0.2f * width;
            float sliderSizeX = width - 2f * sliderLabelSizeX - spacing;

            for (int sliderIndex = 0; sliderIndex < sliderKeys.Count; ++sliderIndex)
            {
                AddNewLine(2f);

                OpLabel opLabel = sliderTextLabelsLeft[sliderIndex];
                opLabel.pos = new Vector2(marginX.x, pos.y + 5f);
                opLabel.size = new Vector2(sliderLabelSizeX, fontHeight);
                tab.AddItems(opLabel);

                OpSlider slider = new(new Vector2(sliderCenter - 0.5f * sliderSizeX, pos.y), sliderKeys[sliderIndex], sliderRanges[sliderIndex], length: (int)sliderSizeX, defaultValue: sliderDefaultValues[sliderIndex])
                {
                    size = new Vector2(sliderSizeX, fontHeight),
                    description = sliderDescriptions[sliderIndex]
                };
                tab.AddItems(slider);

                opLabel = sliderTextLabelsRight[sliderIndex];
                opLabel.pos = new Vector2(sliderCenter + 0.5f * sliderSizeX + 0.5f * spacing, pos.y + 5f);
                opLabel.size = new Vector2(sliderLabelSizeX, fontHeight);
                tab.AddItems(opLabel);

                AddTextLabel(sliderMainTextLabels[sliderIndex]);
                DrawTextLabels(ref tab);

                if (sliderIndex < sliderKeys.Count - 1)
                {
                    AddNewLine();
                }
            }

            sliderKeys.Clear();
            sliderRanges.Clear();
            sliderDefaultValues.Clear();
            sliderDescriptions.Clear();

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