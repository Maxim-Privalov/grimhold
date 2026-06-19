extends Control

const PATH_MAIN_MENU := "res://Scenes/UI/Menu/MainMenu.tscn"
const PATH_GAMEPLAY := "res://Scenes/Gameplay/Gameplay.tscn" 

const ACCENT := Color(0.82, 0.30, 0.18)
const ACCENT_CORRUPT := Color(0.55, 0.20, 0.65)
const TEXT_DIM := Color(0.62, 0.56, 0.52)
const PANEL_BG := Color(0.05, 0.035, 0.04, 0.92)
const PANEL_BORDER_EMPTY := Color(0.22, 0.18, 0.18)
const SLOT_COUNT := 2

var _mode: String = "continue"
var _confirm_panel: PanelContainer
var _pending_slot: int = -1

func _ready() -> void:
	set_anchors_preset(Control.PRESET_FULL_RECT)
	if get_tree().has_meta("save_slots_mode"):
		_mode = get_tree().get_meta("save_slots_mode")
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
	back_btn.pressed.connect(func(): SceneManager.change_scene(PATH_MAIN_MENU))
	top_bar.add_child(back_btn)

	var title := Label.new()
	title.text = "НОВЫЙ ПОХОД" if _mode == "new" else "ПРОДОЛЖИТЬ"
	title.add_theme_font_size_override("font_size", 26)
	title.add_theme_color_override("font_color", TEXT_DIM)
	title.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	title.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	top_bar.add_child(title)

	var spacer := Control.new()
	spacer.custom_minimum_size = Vector2(140, 0)
	top_bar.add_child(spacer)

	var center := CenterContainer.new()
	center.size_flags_vertical = Control.SIZE_EXPAND_FILL
	center.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	root.add_child(center)

	var slots_box := HBoxContainer.new()
	slots_box.add_theme_constant_override("separation", 32)
	center.add_child(slots_box)

	for i in range(SLOT_COUNT):
		slots_box.add_child(_build_slot_panel(i))

	_confirm_panel = _build_confirm_panel()
	_confirm_panel.visible = false
	add_child(_confirm_panel)


func _build_slot_panel(slot_index: int) -> Control:
	var info = SaveSystem.get_slot_info(slot_index)

	var panel := PanelContainer.new()
	panel.custom_minimum_size = Vector2(300, 360)
	var style := StyleBoxFlat.new()
	style.bg_color = PANEL_BG
	style.border_color = ACCENT if info != null else PANEL_BORDER_EMPTY
	style.set_border_width_all(2)
	style.set_corner_radius_all(2)
	panel.add_theme_stylebox_override("panel", style)

	var inner := VBoxContainer.new()
	inner.alignment = BoxContainer.ALIGNMENT_CENTER
	inner.add_theme_constant_override("separation", 10)
	panel.add_child(inner)

	var header := Label.new()
	header.text = "СЛОТ %d" % (slot_index + 1)
	header.add_theme_font_size_override("font_size", 16)
	header.add_theme_color_override("font_color", TEXT_DIM)
	header.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	inner.add_child(header)

	var icon := Label.new()
	icon.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	icon.add_theme_font_size_override("font_size", 40)
	inner.add_child(icon)

	var body := Label.new()
	body.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	body.autowrap_mode = TextServer.AUTOWRAP_WORD
	inner.add_child(body)

	var action_btn := Button.new()
	action_btn.custom_minimum_size = Vector2(180, 40)
	inner.add_child(action_btn)

	if info != null:
		icon.text = "✠"
		icon.add_theme_color_override("font_color", ACCENT)
		body.text = "Глава %s\n%s" % [str(info.get("chapter", "?")), str(info.get("created_at", ""))]
		body.add_theme_color_override("font_color", TEXT_DIM)
		if _mode == "continue":
			action_btn.text = "Продолжить"
			action_btn.pressed.connect(_on_continue_slot.bind(slot_index))
		else:
			action_btn.text = "Перезаписать"
			action_btn.pressed.connect(_on_ask_overwrite.bind(slot_index))
	else:
		icon.text = "✦"
		icon.add_theme_color_override("font_color", PANEL_BORDER_EMPTY)
		body.text = "Тлен и пустота"
		body.add_theme_color_override("font_color", Color(0.4, 0.36, 0.34))
		if _mode == "new":
			action_btn.text = "Начать здесь"
			action_btn.pressed.connect(_on_start_new.bind(slot_index))
		else:
			action_btn.text = "Нет данных"
			action_btn.disabled = true

	return panel


func _build_confirm_panel() -> PanelContainer:
	var panel := PanelContainer.new()
	panel.set_anchors_and_offsets_preset(Control.PRESET_CENTER)
	panel.custom_minimum_size = Vector2(360, 160)
	var style := StyleBoxFlat.new()
	style.bg_color = Color(0.04, 0.03, 0.035, 0.97)
	style.border_color = ACCENT_CORRUPT
	style.set_border_width_all(2)
	style.set_corner_radius_all(2)
	panel.add_theme_stylebox_override("panel", style)

	var box := VBoxContainer.new()
	box.alignment = BoxContainer.ALIGNMENT_CENTER
	box.add_theme_constant_override("separation", 16)
	panel.add_child(box)

	var label := Label.new()
	label.text = "Стереть память об этом походе и начать заново?"
	label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	label.autowrap_mode = TextServer.AUTOWRAP_WORD
	label.add_theme_color_override("font_color", TEXT_DIM)
	box.add_child(label)

	var row := HBoxContainer.new()
	row.alignment = BoxContainer.ALIGNMENT_CENTER
	row.add_theme_constant_override("separation", 20)
	box.add_child(row)

	var yes_btn := Button.new()
	yes_btn.text = "Да"
	yes_btn.custom_minimum_size = Vector2(100, 36)
	yes_btn.add_theme_color_override("font_color", ACCENT)
	yes_btn.pressed.connect(_on_confirm_overwrite)
	row.add_child(yes_btn)

	var no_btn := Button.new()
	no_btn.text = "Отмена"
	no_btn.custom_minimum_size = Vector2(100, 36)
	no_btn.add_theme_color_override("font_color", TEXT_DIM)
	no_btn.pressed.connect(func():
		_confirm_panel.visible = false
		_pending_slot = -1
	)
	row.add_child(no_btn)

	return panel


func _on_start_new(slot_index: int) -> void:
	SaveSystem.create_new_save(slot_index)
	get_tree().set_meta("active_save_slot", slot_index)
	SceneManager.change_scene(PATH_GAMEPLAY)


func _on_continue_slot(slot_index: int) -> void:
	get_tree().set_meta("active_save_slot", slot_index)
	SceneManager.change_scene(PATH_GAMEPLAY)


func _on_ask_overwrite(slot_index: int) -> void:
	_pending_slot = slot_index
	_confirm_panel.visible = true


func _on_confirm_overwrite() -> void:
	_confirm_panel.visible = false
	if _pending_slot >= 0:
		_on_start_new(_pending_slot)
