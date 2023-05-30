using System.Collections.Generic;
using Menu.Remix.MixedUI;
using UnityEngine;

using static MapOptions.MainMod;
using static MapOptions.MapMod;

namespace MapOptions;

public class MainModOptions : OptionInterface
{
    public static MainModOptions instance = new();

    //
    // options
    //

    public static Configurable<bool> aerial_map = instance.config.Bind("aerial_map", defaultValue: false, new ConfigurableInfo("The default map shader is used in Chimney Canopy and Sky Islands instead of the aerial map shader.", null, "", "Aerial Map"));
    public static Configurable<bool> creature_symbols = instance.config.Bind("creature_symbols", defaultValue: true, new ConfigurableInfo("Creature symbols are added to the map. These symbols display what creature types are present in each room.", null, "", "Creature Symbols"));
    public static Configurable<bool> item_tracker = instance.config.Bind("item_tracker", defaultValue: true, new ConfigurableInfo("Tracked key items are shown on the map even when the option 'Slug Senses' is disabled. The option 'Key item tracking' needs to be enabled in Rain World Remix.", null, "", "Item Tracker"));

    public static Configurable<bool> layer_focus = instance.config.Bind("layer_focus", defaultValue: false, new ConfigurableInfo("Only the active layer is displayed on the map.", null, "", "Layer Focus"));
    public static Configurable<bool> shadow_sprites = instance.config.Bind("shadow_sprites", defaultValue: false, new ConfigurableInfo("Draws shadows for creature and slugcat symbols.", null, "", "Shadow Sprites"));
    public static Configurable<bool> skip_fade = instance.config.Bind("skip_fade", defaultValue: false, new ConfigurableInfo("Pressing the map button shows the map with no delay.", null, "", "Skip Fade In/Out"));

    public static Configurable<bool> slugcat_symbols = instance.config.Bind("slugcat_symbols", defaultValue: true, new ConfigurableInfo("Draws a slugcat sprite on the map instead of a red circle. When Jolly Co-Op is enabled, draws a sprite for each player.", null, "", "Slugcat Symbols"));
    public static Configurable<bool> uncover_region = instance.config.Bind("uncover_region", defaultValue: false, new ConfigurableInfo("Once loaded into the game the whole region map gets uncovered.\nWARNING: This progress is saved (even without completing a cycle). Turning this option off after saving will *not* remove the gained progress.", null, "", "Uncover Region"));
    public static Configurable<bool> uncover_room = instance.config.Bind("uncover_room", defaultValue: true, new ConfigurableInfo("When the player enters a room the whole room gets uncovered instead of just the area around slugcat.", null, "", "Uncover Room"));

    public static Configurable<int> zoom_slider = instance.config.Bind("zoom_slider", defaultValue: 10, new ConfigurableInfo("The default value is 100% (10) zoom. Each value (5-15) corresponds to 10*value% (50%-150%) zoom..", new ConfigAcceptableRange<int>(5, 15), "", "Zoom Level (10)"));
    public static Configurable<int> creature_symbol_scale = instance.config.Bind("creature_symbol_scale", defaultValue: 10, new ConfigurableInfo("The default value is 100% (10). Each value (5-20) corresponds to 10*value% (50%-200%).", new ConfigAcceptableRange<int>(5, 20), "", "Creature Symbol Size (10)"));
    public static Configurable<int> slugcat_symbol_scale = instance.config.Bind("slugcat_symbol_scale", defaultValue: 10, new ConfigurableInfo("The default value is 100% (10). Each value (5-20) corresponds to 10*value% (50%-200%).", new ConfigAcceptableRange<int>(5, 20), "", "Slugcat Symbol Size (10)"));
    public static Configurable<int> reveal_speed_multiplier = instance.config.Bind("reveal_speed_multiplier", defaultValue: 1, new ConfigurableInfo("The default value is one. For a given value X the map is revealed X-times as fast.\nIf the maximum value is selected then opening the map displays known areas instantly instead of revealing them gradually.", new ConfigAcceptableRange<int>(1, 10), "", "Reveal Speed Multiplier (1)"));

    //
    // parameters
    //

    private readonly float font_height = 20f;
    private readonly float spacing = 20f;
    private readonly int number_of_check_boxes = 3;
    private readonly float check_box_size = 24f;
    private float Check_Box_With_Spacing => check_box_size + 0.25f * spacing;

    //
    // variables
    //

    private Vector2 margin_x = new();
    private Vector2 position = new();
    private readonly List<OpLabel> text_labels = new();
    private readonly List<float> box_end_positions = new();

    private readonly List<Configurable<bool>> check_box_configurables = new();
    private readonly List<OpLabel> check_boxes_text_labels = new();

    private readonly List<Configurable<int>> slider_configurables = new();
    private readonly List<string> slider_main_text_labels = new();
    private readonly List<OpLabel> slider_text_labels_left = new();
    private readonly List<OpLabel> slider_text_labels_right = new();

    //
    // main
    //

    private MainModOptions() => OnConfigChanged += MainModOptions_OnConfigChanged;

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
        AddTextLabel("Version " + version, FLabelAlignment.Left);
        AddTextLabel("by " + author, FLabelAlignment.Right);
        DrawTextLabels(ref Tabs[tabIndex]);

        // Content //
        AddNewLine();
        AddBox();

        AddSlider(zoom_slider, (string)zoom_slider.info.Tags[0], "50%", "150%");
        DrawSliders(ref Tabs[tabIndex]);

        AddNewLine();

        AddCheckBox(aerial_map, (string)aerial_map.info.Tags[0]);
        AddCheckBox(creature_symbols, (string)creature_symbols.info.Tags[0]);
        AddCheckBox(item_tracker, (string)item_tracker.info.Tags[0]);

        AddCheckBox(layer_focus, (string)layer_focus.info.Tags[0]);
        AddCheckBox(shadow_sprites, (string)shadow_sprites.info.Tags[0]);
        AddCheckBox(skip_fade, (string)skip_fade.info.Tags[0]);

        AddCheckBox(slugcat_symbols, (string)slugcat_symbols.info.Tags[0]);
        AddCheckBox(uncover_region, (string)uncover_region.info.Tags[0]);
        AddCheckBox(uncover_room, (string)uncover_room.info.Tags[0]);

        DrawCheckBoxes(ref Tabs[tabIndex]);

        AddNewLine();

        AddSlider(creature_symbol_scale, (string)creature_symbol_scale.info.Tags[0], "50%", "200%");
        AddSlider(slugcat_symbol_scale, (string)slugcat_symbol_scale.info.Tags[0], "50%", "200%");
        AddSlider(reveal_speed_multiplier, (string)reveal_speed_multiplier.info.Tags[0], "1", "10");
        DrawSliders(ref Tabs[tabIndex]);

        DrawBox(ref Tabs[tabIndex]);
    }

    public void MainModOptions_OnConfigChanged()
    {
        Debug.Log("MapOptions: Creature_Symbols_Scale " + Creature_Symbols_Scale);
        Debug.Log("MapOptions: Map_Scale " + Map_Scale);
        Debug.Log("MapOptions: Slugcat_Symbols_Scale " + Slugcat_Symbols_Scale);

        Debug.Log("MapOptions: Reveal_Speed_Multiplier " + Reveal_Speed_Multiplier);
        Debug.Log("MapOptions: Can_Instant_Reveal " + Can_Instant_Reveal);

        Debug.Log("MapOptions: Option_AerialMap " + Option_AerialMap);
        Debug.Log("MapOptions: Option_CreatureSymbols " + Option_CreatureSymbols);
        Debug.Log("MapOptions: Option_ItemTracker " + Option_ItemTracker);

        Debug.Log("MapOptions: Option_LayerFocus " + Option_LayerFocus);
        Debug.Log("MapOptions: Option_ShadowSprites " + Option_ShadowSprites);
        Debug.Log("MapOptions: Option_SkipFade " + Option_SkipFade);

        Debug.Log("MapOptions: Option_SlugcatSymbols " + Option_SlugcatSymbols);
        Debug.Log("MapOptions: Option_UncoverRegion " + Option_UncoverRegion);
        Debug.Log("MapOptions: Option_UncoverRoom " + Option_UncoverRoom);
    }

    //
    // private
    //

    private void InitializeMarginAndPos()
    {
        margin_x = new Vector2(50f, 550f);
        position = new Vector2(50f, 600f);
    }

    private void AddNewLine(float spacingModifier = 1f)
    {
        position.x = margin_x.x; // left margin
        position.y -= spacingModifier * spacing;
    }

    private void AddBox()
    {
        margin_x += new Vector2(spacing, -spacing);
        box_end_positions.Add(position.y); // end position > start position
        AddNewLine();
    }

    private void DrawBox(ref OpTab tab)
    {
        margin_x += new Vector2(-spacing, spacing);
        AddNewLine();

        float boxWidth = margin_x.y - margin_x.x;
        int lastIndex = box_end_positions.Count - 1;

        tab.AddItems(new OpRect(position, new Vector2(boxWidth, box_end_positions[lastIndex] - position.y)));
        box_end_positions.RemoveAt(lastIndex);
    }

    private void AddCheckBox(Configurable<bool> configurable, string text)
    {
        check_box_configurables.Add(configurable);
        check_boxes_text_labels.Add(new OpLabel(new Vector2(), new Vector2(), text, FLabelAlignment.Left));
    }

    private void DrawCheckBoxes(ref OpTab tab) // changes pos.y but not pos.x
    {
        if (check_box_configurables.Count != check_boxes_text_labels.Count) return;

        float width = margin_x.y - margin_x.x;
        float elementWidth = (width - (number_of_check_boxes - 1) * 0.5f * spacing) / number_of_check_boxes;
        position.y -= check_box_size;
        float _posX = position.x;

        for (int checkBoxIndex = 0; checkBoxIndex < check_box_configurables.Count; ++checkBoxIndex)
        {
            Configurable<bool> configurable = check_box_configurables[checkBoxIndex];
            OpCheckBox checkBox = new(configurable, new Vector2(_posX, position.y))
            {
                description = configurable.info?.description ?? ""
            };
            tab.AddItems(checkBox);
            _posX += Check_Box_With_Spacing;

            OpLabel checkBoxLabel = check_boxes_text_labels[checkBoxIndex];
            checkBoxLabel.pos = new Vector2(_posX, position.y + 2f);
            checkBoxLabel.size = new Vector2(elementWidth - Check_Box_With_Spacing, font_height);
            tab.AddItems(checkBoxLabel);

            if (checkBoxIndex < check_box_configurables.Count - 1)
            {
                if ((checkBoxIndex + 1) % number_of_check_boxes == 0)
                {
                    AddNewLine();
                    position.y -= check_box_size;
                    _posX = position.x;
                }
                else
                {
                    _posX += elementWidth - Check_Box_With_Spacing + 0.5f * spacing;
                }
            }
        }

        check_box_configurables.Clear();
        check_boxes_text_labels.Clear();
    }

    private void AddSlider(Configurable<int> configurable, string text, string sliderTextLeft = "", string sliderTextRight = "")
    {
        slider_configurables.Add(configurable);
        slider_main_text_labels.Add(text);
        slider_text_labels_left.Add(new OpLabel(new Vector2(), new Vector2(), sliderTextLeft, alignment: FLabelAlignment.Right)); // set pos and size when drawing
        slider_text_labels_right.Add(new OpLabel(new Vector2(), new Vector2(), sliderTextRight, alignment: FLabelAlignment.Left));
    }

    private void DrawSliders(ref OpTab tab)
    {
        if (slider_configurables.Count != slider_main_text_labels.Count) return;
        if (slider_configurables.Count != slider_text_labels_left.Count) return;
        if (slider_configurables.Count != slider_text_labels_right.Count) return;

        float width = margin_x.y - margin_x.x;
        float sliderCenter = margin_x.x + 0.5f * width;
        float sliderLabelSizeX = 0.2f * width;
        float sliderSizeX = width - 2f * sliderLabelSizeX - spacing;

        for (int sliderIndex = 0; sliderIndex < slider_configurables.Count; ++sliderIndex)
        {
            AddNewLine(2f);

            OpLabel opLabel = slider_text_labels_left[sliderIndex];
            opLabel.pos = new Vector2(margin_x.x, position.y + 5f);
            opLabel.size = new Vector2(sliderLabelSizeX, font_height);
            tab.AddItems(opLabel);

            Configurable<int> configurable = slider_configurables[sliderIndex];
            OpSlider slider = new(configurable, new Vector2(sliderCenter - 0.5f * sliderSizeX, position.y), (int)sliderSizeX)
            {
                size = new Vector2(sliderSizeX, font_height),
                description = configurable.info?.description ?? ""
            };
            tab.AddItems(slider);

            opLabel = slider_text_labels_right[sliderIndex];
            opLabel.pos = new Vector2(sliderCenter + 0.5f * sliderSizeX + 0.5f * spacing, position.y + 5f);
            opLabel.size = new Vector2(sliderLabelSizeX, font_height);
            tab.AddItems(opLabel);

            AddTextLabel(slider_main_text_labels[sliderIndex]);
            DrawTextLabels(ref tab);

            if (sliderIndex < slider_configurables.Count - 1)
            {
                AddNewLine();
            }
        }

        slider_configurables.Clear();
        slider_main_text_labels.Clear();
        slider_text_labels_left.Clear();
        slider_text_labels_right.Clear();
    }

    private void AddTextLabel(string text, FLabelAlignment alignment = FLabelAlignment.Center, bool bigText = false)
    {
        float textHeight = (bigText ? 2f : 1f) * font_height;
        if (text_labels.Count == 0)
        {
            position.y -= textHeight;
        }

        OpLabel textLabel = new(new Vector2(), new Vector2(20f, textHeight), text, alignment, bigText) // minimal size.x = 20f
        {
            autoWrap = true
        };
        text_labels.Add(textLabel);
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
        if (text_labels.Count == 0)
        {
            return;
        }

        float width = (margin_x.y - margin_x.x) / text_labels.Count;
        foreach (OpLabel textLabel in text_labels)
        {
            textLabel.pos = position;
            textLabel.size += new Vector2(width - 20f, 0.0f);
            tab.AddItems(textLabel);
            position.x += width;
        }

        position.x = margin_x.x;
        text_labels.Clear();
    }
}