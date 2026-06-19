extends Control

const PATH_SAVE_SLOTS := "res://Scenes/UI/Menu/SaveSlotsMenu.tscn"
const PATH_SETTINGS := "res://Scenes/UI/Menu/SettingsMenu.tscn"

const ACCENT := Color(0.82, 0.30, 0.18)        
const ACCENT_CORRUPT := Color(0.55, 0.20, 0.65) 
const TEXT_DIM := Color(0.62, 0.56, 0.52)        
const TEXT_FAINT := Color(0.40, 0.34, 0.34)

var _buttons: Array[Button] = []

func _ready() -> void:
	set_anchors_preset(Control.PRESET_FULL_RECT)
	_build_ui()


func _build_ui() -> void:
	var bg := preload("res://Scenes/UI/Menu/GridBackground.gd").new()
	add_child(bg)

	var root := VBoxContainer.new()
	root.set_anchors_preset(Control.PRESET_CENTER)
	root.position = Vector2(0, 0)
	root.alignment = BoxContainer.ALIGNMENT_CENTER
	root.add_theme_constant_override("separation", 28)
	add_child(root)
	root.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)

	var spacer_top := Control.new()
	spacer_top.size_flags_vertical = Control.SIZE_EXPAND_FILL
	root.add_child(spacer_top)

	var title_wrap := VBoxContainer.new()
	title_wrap.alignment = BoxContainer.ALIGNMENT_CENTER
	title_wrap.add_theme_constant_override("separation", 6)
	root.add_child(title_wrap)

	var title := Label.new()
	title.text = "ГИЛЬДЕЙСКИЙ ЛАГЕРЬ"
	title.add_theme_font_size_override("font_size", 52)
	title.add_theme_color_override("font_color", TEXT_DIM)
	title.add_theme_color_override("font_shadow_color", Color(ACCENT, 0.55))
	title.add_theme_constant_override("shadow_offset_x", 0)
	title.add_theme_constant_override("shadow_offset_y", 0)
	title.add_theme_constant_override("shadow_outline_size", 10)
	title.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	title_wrap.add_child(title)

	var subtitle := Label.new()
	subtitle.text = "у   в р а т   б е з д н ы"
	subtitle.add_theme_font_size_override("font_size", 14)
	subtitle.add_theme_color_override("font_color", ACCENT_CORRUPT)
	subtitle.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	title_wrap.add_child(subtitle)

	var gap1 := Control.new()
	gap1.custom_minimum_size = Vector2(0, 40)
	root.add_child(gap1)

	var menu_box := VBoxContainer.new()
	menu_box.alignment = BoxContainer.ALIGNMENT_CENTER
	menu_box.add_theme_constant_override("separation", 14)
	menu_box.custom_minimum_size = Vector2(320, 0)
	menu_box.size_flags_horizontal = Control.SIZE_SHRINK_CENTER
	root.add_child(menu_box)

	_add_menu_button(menu_box, "Новый поход", _on_new_game_pressed)
	_add_menu_button(menu_box, "Продолжить", _on_continue_pressed)
	_add_menu_button(menu_box, "Настройки", _on_settings_pressed)
	_add_menu_button(menu_box, "Покинуть лагерь", _on_quit_pressed)

	var spacer_bottom := Control.new()
	spacer_bottom.size_flags_vertical = Control.SIZE_EXPAND_FILL
	root.add_child(spacer_bottom)

	var flavor := Label.new()
	flavor.text = "\"Континент пал. Лагерь — последнее, что ещё держится.\""
	flavor.add_theme_font_size_override("font_size", 13)
	flavor.add_theme_color_override("font_color", Color(TEXT_FAINT, 0.8))
	flavor.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	root.add_child(flavor)

	var gap2 := Control.new()
	gap2.custom_minimum_size = Vector2(0, 18)
	root.add_child(gap2)

	var version := Label.new()
	version.text = "build 0.1.0"
	version.add_theme_font_size_override("font_size", 12)
	version.add_theme_color_override("font_color", Color(TEXT_FAINT, 0.5))
	version.horizontal_alignment = HORIZONTAL_ALIGNMENT_RIGHT
	version.anchor_left = 1.0
	version.anchor_right = 1.0
	version.offset_left = -160
	version.offset_top = -30
	version.offset_bottom = -8
	version.offset_right = -16
	add_child(version)


func _add_menu_button(parent: Control, text: String, callback: Callable) -> void:
	var btn := Button.new()
	btn.text = "† " + text
	btn.custom_minimum_size = Vector2(0, 48)
	btn.add_theme_font_size_override("font_size", 20)
	btn.add_theme_color_override("font_color", TEXT_DIM)
	btn.add_theme_color_override("font_hover_color", ACCENT)
	btn.add_theme_color_override("font_focus_color", ACCENT)
	btn.alignment = HORIZONTAL_ALIGNMENT_LEFT
	btn.flat = true
	btn.focus_mode = Control.FOCUS_ALL
	parent.add_child(btn)
	btn.pressed.connect(callback)
	btn.mouse_entered.connect(_on_button_hover.bind(btn))
	btn.mouse_exited.connect(_on_button_unhover.bind(btn))
	_buttons.append(btn)


func _on_button_hover(btn: Button) -> void:
	var tw := create_tween()
	tw.tween_property(btn, "position:x", 12.0, 0.12).set_trans(Tween.TRANS_SINE)


func _on_button_unhover(btn: Button) -> void:
	var tw := create_tween()
	tw.tween_property(btn, "position:x", 0.0, 0.12).set_trans(Tween.TRANS_SINE)


func _on_new_game_pressed() -> void:
	SaveSlotsMenu_mode_new()
	SceneManager.change_scene(PATH_SAVE_SLOTS)


func _on_continue_pressed() -> void:
	SaveSlotsMenu_mode_continue()
	SceneManager.change_scene(PATH_SAVE_SLOTS)


func _on_settings_pressed() -> void:
	SceneManager.change_scene(PATH_SETTINGS)


func _on_quit_pressed() -> void:
	SceneManager.quit_game()


func SaveSlotsMenu_mode_new() -> void:
	get_tree().set_meta("save_slots_mode", "new")

func SaveSlotsMenu_mode_continue() -> void:
	get_tree().set_meta("save_slots_mode", "continue")
