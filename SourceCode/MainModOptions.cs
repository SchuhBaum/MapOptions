using Menu.Remix.MixedUI;
using System.Collections.Generic;
using UnityEngine;
using static MapOptions.MainMod;
using static MapOptions.MapMod;
using static MapOptions.ProcessManagerMod;

namespace MapOptions;

public class MainModOptions : OptionInterface {
    public static MainModOptions main_mod_options = new();

    //
    // options
    //

    public static Configurable<bool> aerial_map = main_mod_options.config.Bind("aerial_map", defaultValue: false, new ConfigurableInfo("The default map shader is used in Chimney Canopy and Sky Islands instead of the aerial map shader.", null, "", "Aerial Map"));
    public static Configurable<bool> creature_symbols = main_mod_options.config.Bind("creature_symbols", defaultValue: true, new ConfigurableInfo("Creature symbols are added to the map. These symbols display what creature types are present in each room.", null, "", "Creature Symbols"));
    public static Configurable<bool> item_tracker = main_mod_options.config.Bind("item_tracker", defaultValue: true, new ConfigurableInfo("Tracked key items are shown on the map even when the option 'Slug Senses' is disabled. The option 'Key item tracking' needs to be enabled in Rain World Remix.", null, "", "Item Tracker"));

    public static Configurable<bool> layer_focus = main_mod_options.config.Bind("layer_focus", defaultValue: false, new ConfigurableInfo("Only the active layer is displayed on the map.", null, "", "Layer Focus"));
    public static Configurable<bool> shadow_sprites = main_mod_options.config.Bind("shadow_sprites", defaultValue: false, new ConfigurableInfo("Draws shadows for creature and slugcat symbols.", null, "", "Shadow Sprites"));
    public static Configurable<bool> skip_fade = main_mod_options.config.Bind("skip_fade", defaultValue: false, new ConfigurableInfo("Pressing the map button shows the map with no delay.", null, "", "Skip Fade In/Out"));

    public static Configurable<bool> slugcat_symbols = main_mod_options.config.Bind("slugcat_symbols", defaultValue: true, new ConfigurableInfo("Draws a slugcat sprite on the map instead of a red circle. When Jolly Co-Op is enabled, draws a sprite for each player.", null, "", "Slugcat Symbols"));
    public static Configurable<bool> uncover_region = main_mod_options.config.Bind("uncover_region", defaultValue: false, new ConfigurableInfo("Once loaded into the game the whole region map gets uncovered.\nWARNING: This progress is saved (even without completing a cycle). Turning this option off after saving will *not* remove the gained progress.", null, "", "Uncover Region"));
    public static Configurable<bool> uncover_room = main_mod_options.config.Bind("uncover_room", defaultValue: true, new ConfigurableInfo("When the player enters a room the whole room gets uncovered instead of just the area around slugcat.", null, "", "Uncover Room"));

    public static Configurable<int> zoom_slider = main_mod_options.config.Bind("zoom_slider", defaultValue: 10, new ConfigurableInfo("The default value is 100% (10) zoom. Each value (5-15) corresponds to 10*value% (50%-150%) zoom..", new ConfigAcceptableRange<int>(5, 15), "", "Zoom Level (10)"));
    public static Configurable<int> creature_symbol_scale = main_mod_options.config.Bind("creature_symbol_scale", defaultValue: 10, new ConfigurableInfo("The default value is 100% (10). Each value (5-20) corresponds to 10*value% (50%-200%).", new ConfigAcceptableRange<int>(5, 20), "", "Creature Symbol Size (10)"));
    public static Configurable<int> slugcat_symbol_scale = main_mod_options.config.Bind("slugcat_symbol_scale", defaultValue: 10, new ConfigurableInfo("The default value is 100% (10). Each value (5-20) corresponds to 10*value% (50%-200%).", new ConfigAcceptableRange<int>(5, 20), "", "Slugcat Symbol Size (10)"));
    public static Configurable<int> reveal_speed_multiplier = main_mod_options.config.Bind("reveal_speed_multiplier", defaultValue: 1, new ConfigurableInfo("The default value is one. For a given value X the map is revealed X-times as fast.\nIf the maximum value is selected then opening the map displays known areas instantly instead of revealing them gradually.", new ConfigAcceptableRange<int>(1, 10), "", "Reveal Speed Multiplier (1)"));

    //
    // parameters
    //

    private readonly float _font_height = 20f;
    private readonly float _spacing = 20f;
    private readonly int _number_of_check_boxes = 3;
    private readonly float _check_box_size = 24f;
    private float Check_Box_With_Spacing => _check_box_size + 0.25f * _spacing;

    //
    // variables
    //

    private Vector2 _margin_x = new();
    private Vector2 _position = new();
    private readonly List<OpLabel> _text_labels = new();
    private readonly List<float> _box_end_positions = new();

    private readonly List<Configurable<bool>> _check_box_configurables = new();
    private readonly List<OpLabel> _check_boxes_text_labels = new();

    private readonly List<Configurable<int>> _slider_configurables = new();
    private readonly List<string> _slider_main_text_labels = new();
    private readonly List<OpLabel> _slider_text_labels_left = new();
    private readonly List<OpLabel> _slider_text_labels_right = new();

    //
    // main
    //

    private MainModOptions() {
        On.OptionInterface._SaveConfigFile -= Save_Config_File;
        On.OptionInterface._SaveConfigFile += Save_Config_File;
    }

    private void Save_Config_File(On.OptionInterface.orig__SaveConfigFile orig, OptionInterface option_interface) {
        // the event OnConfigChange is triggered too often;
        // it is triggered when you click on the mod name in the
        // remix menu;
        // initializing the hooks takes like half a second;
        // I don't want to do that too often;

        orig(option_interface);
        if (option_interface != main_mod_options) return;
        Debug.Log(mod_id + ": Save_Config_File.");
        Initialize_Option_Specific_Hooks();
    }

    //
    // public
    //

    public override void Initialize() {
        base.Initialize();
        Tabs = new OpTab[1];

        int tab_index = 0;
        Tabs[tab_index] = new OpTab(main_mod_options, "Options");
        InitializeMarginAndPos();

        // Title
        AddNewLine();
        AddTextLabel("MapOptions Mod", big_text: true);
        DrawTextLabels(ref Tabs[tab_index]);

        // Subtitle
        AddNewLine(0.5f);
        AddTextLabel("Version " + version, FLabelAlignment.Left);
        AddTextLabel("by " + author, FLabelAlignment.Right);
        DrawTextLabels(ref Tabs[tab_index]);

        // Content //
        AddNewLine();
        AddBox();

        AddSlider(zoom_slider, (string)zoom_slider.info.Tags[0], "50%", "150%");
        DrawSliders(ref Tabs[tab_index]);

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

        DrawCheckBoxes(ref Tabs[tab_index]);

        AddNewLine();

        AddSlider(creature_symbol_scale, (string)creature_symbol_scale.info.Tags[0], "50%", "200%");
        AddSlider(slugcat_symbol_scale, (string)slugcat_symbol_scale.info.Tags[0], "50%", "200%");
        AddSlider(reveal_speed_multiplier, (string)reveal_speed_multiplier.info.Tags[0], "1", "10");
        DrawSliders(ref Tabs[tab_index]);

        DrawBox(ref Tabs[tab_index]);
    }

    public void Log_All_Options() {
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

    private void InitializeMarginAndPos() {
        _margin_x = new Vector2(50f, 550f);
        _position = new Vector2(50f, 600f);
    }

    private void AddNewLine(float spacing_modifier = 1f) {
        _position.x = _margin_x.x; // left margin
        _position.y -= spacing_modifier * _spacing;
    }

    private void AddBox() {
        _margin_x += new Vector2(_spacing, -_spacing);
        _box_end_positions.Add(_position.y); // end position > start position
        AddNewLine();
    }

    private void DrawBox(ref OpTab tab) {
        _margin_x += new Vector2(-_spacing, _spacing);
        AddNewLine();

        float box_width = _margin_x.y - _margin_x.x;
        int last_index = _box_end_positions.Count - 1;

        tab.AddItems(new OpRect(_position, new Vector2(box_width, _box_end_positions[last_index] - _position.y)));
        _box_end_positions.RemoveAt(last_index);
    }

    private void AddCheckBox(Configurable<bool> configurable, string text) {
        _check_box_configurables.Add(configurable);
        _check_boxes_text_labels.Add(new OpLabel(new Vector2(), new Vector2(), text, FLabelAlignment.Left));
    }

    private void DrawCheckBoxes(ref OpTab tab) { // changes pos.y but not pos.x
        if (_check_box_configurables.Count != _check_boxes_text_labels.Count) return;

        float width = _margin_x.y - _margin_x.x;
        float element_width = (width - (_number_of_check_boxes - 1) * 0.5f * _spacing) / _number_of_check_boxes;
        _position.y -= _check_box_size;
        float pos_x = _position.x;

        for (int check_box_index = 0; check_box_index < _check_box_configurables.Count; ++check_box_index) {
            Configurable<bool> configurable = _check_box_configurables[check_box_index];
            OpCheckBox check_box = new(configurable, new Vector2(pos_x, _position.y)) {
                description = configurable.info?.description ?? ""
            };
            tab.AddItems(check_box);
            pos_x += Check_Box_With_Spacing;

            OpLabel check_box_label = _check_boxes_text_labels[check_box_index];
            check_box_label.pos = new Vector2(pos_x, _position.y + 2f);
            check_box_label.size = new Vector2(element_width - Check_Box_With_Spacing, _font_height);
            tab.AddItems(check_box_label);

            if (check_box_index < _check_box_configurables.Count - 1) {
                if ((check_box_index + 1) % _number_of_check_boxes == 0) {
                    AddNewLine();
                    _position.y -= _check_box_size;
                    pos_x = _position.x;
                } else {
                    pos_x += element_width - Check_Box_With_Spacing + 0.5f * _spacing;
                }
            }
        }

        _check_box_configurables.Clear();
        _check_boxes_text_labels.Clear();
    }

    private void AddSlider(Configurable<int> configurable, string text, string slider_text_left = "", string slider_text_right = "") {
        _slider_configurables.Add(configurable);
        _slider_main_text_labels.Add(text);
        _slider_text_labels_left.Add(new OpLabel(new Vector2(), new Vector2(), slider_text_left, alignment: FLabelAlignment.Right)); // set pos and size when drawing
        _slider_text_labels_right.Add(new OpLabel(new Vector2(), new Vector2(), slider_text_right, alignment: FLabelAlignment.Left));
    }

    private void DrawSliders(ref OpTab tab) {
        if (_slider_configurables.Count != _slider_main_text_labels.Count) return;
        if (_slider_configurables.Count != _slider_text_labels_left.Count) return;
        if (_slider_configurables.Count != _slider_text_labels_right.Count) return;

        float width = _margin_x.y - _margin_x.x;
        float slider_center = _margin_x.x + 0.5f * width;
        float slider_label_size_x = 0.2f * width;
        float slider_size_x = width - 2f * slider_label_size_x - _spacing;

        for (int slider_index = 0; slider_index < _slider_configurables.Count; ++slider_index) {
            AddNewLine(2f);

            OpLabel op_label = _slider_text_labels_left[slider_index];
            op_label.pos = new Vector2(_margin_x.x, _position.y + 5f);
            op_label.size = new Vector2(slider_label_size_x, _font_height);
            tab.AddItems(op_label);

            Configurable<int> configurable = _slider_configurables[slider_index];
            OpSlider slider = new(configurable, new Vector2(slider_center - 0.5f * slider_size_x, _position.y), (int)slider_size_x) {
                size = new Vector2(slider_size_x, _font_height),
                description = configurable.info?.description ?? ""
            };
            tab.AddItems(slider);

            op_label = _slider_text_labels_right[slider_index];
            op_label.pos = new Vector2(slider_center + 0.5f * slider_size_x + 0.5f * _spacing, _position.y + 5f);
            op_label.size = new Vector2(slider_label_size_x, _font_height);
            tab.AddItems(op_label);

            AddTextLabel(_slider_main_text_labels[slider_index]);
            DrawTextLabels(ref tab);

            if (slider_index < _slider_configurables.Count - 1) {
                AddNewLine();
            }
        }

        _slider_configurables.Clear();
        _slider_main_text_labels.Clear();
        _slider_text_labels_left.Clear();
        _slider_text_labels_right.Clear();
    }

    private void AddTextLabel(string text, FLabelAlignment alignment = FLabelAlignment.Center, bool big_text = false) {
        float text_height = (big_text ? 2f : 1f) * _font_height;
        if (_text_labels.Count == 0) {
            _position.y -= text_height;
        }

        OpLabel text_label = new(new Vector2(), new Vector2(20f, text_height), text, alignment, big_text) { // minimal size.x = 20f
            autoWrap = true
        };
        _text_labels.Add(text_label);
    }

    private void DrawTextLabels(ref OpTab tab) {
        if (_text_labels.Count == 0) {
            return;
        }

        float width = (_margin_x.y - _margin_x.x) / _text_labels.Count;
        foreach (OpLabel text_label in _text_labels) {
            text_label.pos = _position;
            text_label.size += new Vector2(width - 20f, 0.0f);
            tab.AddItems(text_label);
            _position.x += width;
        }

        _position.x = _margin_x.x;
        _text_labels.Clear();
    }
}
