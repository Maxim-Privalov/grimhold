extends Control

const PATH_MAIN_MENU := "res://Scenes/UI/Menu/MainMenu.tscn"
const CONFIG_PATH := "user://settings.cfg"

const ACCENT := Color(0.82, 0.30, 0.18)
const TEXT_DIM := Color(0.62, 0.56, 0.52)
const BORDER_FAINT := Color(0.22, 0.18, 0.18)

var _config := ConfigFile.new()

func _ready() -> void:
	set_anchors_preset(Control.PRESET_FULL_RECT)
	_config.load(CONFIG_PATH)
	_build_ui()


func _build_ui() -> void:
	var bg := preload("res://Scenes/UI/Menu/GridBackground.gd").new()
	add_child(bg)

	var root := VBoxContainer.new()
	root.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	root.add_theme_constant_override("separation", 24)
	add_child(root)

	var top_bar := HBoxContainer.new()
	top_bar.custom_minimum_size = Vector2(0, 64)
	root.add_child(top_bar)

	var back_btn := Button.new()
	back_btn.text = "< Назад"
	back_btn.flat = true
	back_btn.add_theme_font_size_override("font_size", 18)
	back_btn.add_theme_color_override("font_color", TEXT_DIM)
	back_btn.add_theme_color_override("font_hover_color", ACCENT)
	back_btn.custom_minimum_size = Vector2(140, 0)
	back_btn.pressed.connect(func():
		_save_config()
		SceneManager.change_scene(PATH_MAIN_MENU)
	)
	top_bar.add_child(back_btn)

	var title := Label.new()
	title.text = "НАСТРОЙКИ"
	title.add_theme_font_size_override("font_size", 26)
	title.add_theme_color_override("font_color", TEXT_DIM)
	title.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	title.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	top_bar.add_child(title)

	var spacer_r := Control.new()
	spacer_r.custom_minimum_size = Vector2(140, 0)
	top_bar.add_child(spacer_r)

	var center := CenterContainer.new()
	center.size_flags_vertical = Control.SIZE_EXPAND_FILL
	root.add_child(center)

	var panel := PanelContainer.new()
	panel.custom_minimum_size = Vector2(480, 0)
	var style := StyleBoxFlat.new()
	style.bg_color = Color(0.05, 0.035, 0.04, 0.92)
	style.border_color = BORDER_FAINT
	style.set_border_width_all(2)
	style.set_corner_radius_all(2)
	style.content_margin_left = 28
	style.content_margin_right = 28
	style.content_margin_top = 24
	style.content_margin_bottom = 24
	panel.add_theme_stylebox_override("panel", style)
	center.add_child(panel)

	var list := VBoxContainer.new()
	list.add_theme_constant_override("separation", 22)
	panel.add_child(list)

	_add_slider_row(list, "Громкость музыки", "music_volume", 0.7)
	_add_slider_row(list, "Громкость звуков", "sfx_volume", 0.8)
	_add_separator(list)
	_add_toggle_row(list, "Полноэкранный режим", "fullscreen", false, _on_fullscreen_toggled)
	_add_separator(list)
	_add_option_row(list, "Язык", "language", ["Русский", "English"], 0)


func _add_separator(parent: Control) -> void:
	var sep := HSeparator.new()
	sep.add_theme_color_override("color", Color(BORDER_FAINT, 0.6))
	parent.add_child(sep)


func _add_slider_row(parent: Control, label_text: String, key: String, default_val: float) -> void:
	var row := VBoxContainer.new()
	row.add_theme_constant_override("separation", 6)
	parent.add_child(row)

	var label := Label.new()
	label.text = label_text
	label.add_theme_color_override("font_color", TEXT_DIM)
	row.add_child(label)

	var slider := HSlider.new()
	slider.min_value = 0.0
	slider.max_value = 1.0
	slider.step = 0.01
	slider.value = float(_config.get_value("settings", key, default_val))
	slider.custom_minimum_size = Vector2(0, 20)
	row.add_child(slider)
	slider.value_changed.connect(func(v):
		_config.set_value("settings", key, v)
	)


func _add_toggle_row(parent: Control, label_text: String, key: String, default_val: bool, callback: Callable) -> void:
	var row := HBoxContainer.new()
	parent.add_child(row)

	var label := Label.new()
	label.text = label_text
	label.add_theme_color_override("font_color", TEXT_DIM)
	label.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	row.add_child(label)

	var toggle := CheckButton.new()
	toggle.button_pressed = bool(_config.get_value("settings", key, default_val))
	row.add_child(toggle)
	toggle.toggled.connect(func(pressed):
		_config.set_value("settings", key, pressed)
		callback.call(pressed)
	)


func _add_option_row(parent: Control, label_text: String, key: String, options: Array, default_index: int) -> void:
	var row := HBoxContainer.new()
	parent.add_child(row)

	var label := Label.new()
	label.text = label_text
	label.add_theme_color_override("font_color", TEXT_DIM)
	label.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	row.add_child(label)

	var option := OptionButton.new()
	for opt in options:
		option.add_item(opt)
	option.selected = int(_config.get_value("settings", key, default_index))
	row.add_child(option)
	option.item_selected.connect(func(idx):
		_config.set_value("settings", key, idx)
	)


func _on_fullscreen_toggled(pressed: bool) -> void:
	if pressed:
		DisplayServer.window_set_mode(DisplayServer.WINDOW_MODE_FULLSCREEN)
	else:
		DisplayServer.window_set_mode(DisplayServer.WINDOW_MODE_WINDOWED)


func _save_config() -> void:
	_config.save(CONFIG_PATH)
