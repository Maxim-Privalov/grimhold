extends Control

# ============================================
# Пути к сценам
# ============================================
const PATH_MAIN_MENU := "res://Scenes/UI/Menu/MainMenu.tscn"
const PATH_GAMEPLAY := "res://Scenes/Gameplay/Gameplay.tscn"

# ============================================
# Цветовая палитра (единая точка правды)
# ============================================
const COLOR_ACCENT        := Color(0.82, 0.30, 0.18)
const COLOR_ACCENT_HOVER  := Color(0.95, 0.45, 0.25)
const COLOR_ACCENT_CORRUPT := Color(0.55, 0.20, 0.65)
const COLOR_TEXT          := Color(0.85, 0.82, 0.78)
const COLOR_TEXT_DIM      := Color(0.62, 0.56, 0.52)
const COLOR_TEXT_FAINT    := Color(0.40, 0.34, 0.34)
const COLOR_PANEL_BG      := Color(0.05, 0.035, 0.04, 0.92)
const COLOR_BORDER_EMPTY  := Color(0.22, 0.18, 0.18)
const COLOR_BORDER_FILLED := COLOR_ACCENT  # слот с данными
const COLOR_OVERLAY       := Color(0.02, 0.02, 0.04, 0.55)
const COLOR_BUTTON_BG     := Color(0.08, 0.07, 0.09, 0.65)
const COLOR_BUTTON_BORDER := Color(0.25, 0.22, 0.25, 0.4)

# ============================================
# Параметры
# ============================================
const SLOT_COUNT := 2

# ============================================
# Состояние
# ============================================
var _mode: String = "continue"  # "continue" | "new"
var _confirm_popup: PanelContainer  # диалог подтверждения
var _pending_overwrite_slot: int = -1


# ============================================
# Жизненный цикл
# ============================================

func _ready() -> void:
	set_anchors_preset(Control.PRESET_FULL_RECT)
	_mode = get_tree().get_meta("save_slots_mode", "continue")
	_build_background()
	_build_content()
	_build_confirm_dialog()


# ============================================
# Построение UI
# ============================================

func _build_background() -> void:
	# Фоновое изображение
	var bg := TextureRect.new()
	bg.set_anchors_preset(Control.PRESET_FULL_RECT)
	bg.texture = preload("res://Assets/Backgrounds/BackgroundMain.png")
	bg.expand_mode = TextureRect.EXPAND_IGNORE_SIZE
	bg.stretch_mode = TextureRect.STRETCH_SCALE
	add_child(bg)

	# Затемняющий оверлей
	var overlay := ColorRect.new()
	overlay.set_anchors_preset(Control.PRESET_FULL_RECT)
	overlay.color = COLOR_OVERLAY
	add_child(overlay)


func _build_content() -> void:
	var root := VBoxContainer.new()
	root.alignment = BoxContainer.ALIGNMENT_CENTER
	root.set_anchors_preset(Control.PRESET_FULL_RECT)
	add_child(root)

	# === Верхняя панель: назад / заголовок / спейсер ===
	root.add_child(_build_top_bar())

	# === Центральная область со слотами ===
	var center := CenterContainer.new()
	center.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	center.size_flags_vertical = Control.SIZE_EXPAND_FILL
	root.add_child(center)

	var slots_row := HBoxContainer.new()
	slots_row.add_theme_constant_override("separation", 32)
	center.add_child(slots_row)

	for i in SLOT_COUNT:
		slots_row.add_child(_create_slot_panel(i))


func _build_top_bar() -> Control:
	var bar := HBoxContainer.new()
	bar.custom_minimum_size = Vector2(0, 64)

	# Кнопка "Назад"
	var back_btn := _make_text_button("< Назад", _on_back_pressed)
	back_btn.custom_minimum_size = Vector2(140, 0)
	bar.add_child(back_btn)

	# Заголовок
	var title := Label.new()
	title.text = "НОВЫЙ ПОХОД" if _mode == "new" else "ПРОДОЛЖИТЬ"
	title.add_theme_font_size_override("font_size", 26)
	title.add_theme_color_override("font_color", COLOR_TEXT_DIM)
	title.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	title.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	bar.add_child(title)

	# Спейсер для баланса
	var spacer := Control.new()
	spacer.custom_minimum_size = Vector2(140, 0)
	bar.add_child(spacer)

	return bar


func _create_slot_panel(slot_index: int) -> Control:
	var save_data = SaveSystem.get_slot_info(slot_index)
	var is_empty := save_data == null

	# --- Контейнер слота ---
	var panel := PanelContainer.new()
	panel.custom_minimum_size = Vector2(300, 360)
	panel.add_theme_stylebox_override("panel", _make_panel_style(is_empty))

	# --- Внутренний вертикальный контейнер ---
	var inner := VBoxContainer.new()
	inner.alignment = BoxContainer.ALIGNMENT_CENTER
	inner.add_theme_constant_override("separation", 10)
	panel.add_child(inner)

	# Заголовок слота
	inner.add_child(_make_label(
		"СЛОТ %d" % (slot_index + 1),
		16, COLOR_TEXT_DIM
	))

	# Иконка
	var icon := _make_label(
		"✠" if not is_empty else "✦",
		40,
		COLOR_ACCENT if not is_empty else COLOR_BORDER_EMPTY
	)
	inner.add_child(icon)

	# Информация о сохранении
	var body := _make_label("", 14, COLOR_TEXT_DIM)
	body.autowrap_mode = TextServer.AUTOWRAP_WORD
	if not is_empty:
		body.text = "Глава %s\n%s" % [
			str(save_data.get("chapter", "?")),
			str(save_data.get("created_at", ""))
		]
	else:
		body.text = "Тлен и пустота"
		body.add_theme_color_override("font_color", Color(0.4, 0.36, 0.34))
	inner.add_child(body)

	# Кнопка действия
	inner.add_child(_make_action_button(slot_index, is_empty))

	return panel


func _make_action_button(slot_index: int, is_empty: bool) -> Button:
	var btn := Button.new()
	btn.custom_minimum_size = Vector2(180, 40)
	btn.flat = false
	btn.alignment = HORIZONTAL_ALIGNMENT_CENTER
	btn.add_theme_font_size_override("font_size", 16)

	# Стили (общие)
	btn.add_theme_stylebox_override("normal", _button_style_normal())
	btn.add_theme_stylebox_override("hover", _button_style_hover())
	btn.add_theme_stylebox_override("pressed", _button_style_pressed())

	if not is_empty:
		if _mode == "continue":
			btn.text = "Продолжить"
			btn.add_theme_color_override("font_color", COLOR_ACCENT)
			btn.add_theme_color_override("font_hover_color", COLOR_ACCENT_HOVER)
			btn.pressed.connect(_on_continue_pressed.bind(slot_index))
		else:
			btn.text = "Перезаписать"
			btn.add_theme_color_override("font_color", COLOR_ACCENT_CORRUPT)
			btn.add_theme_color_override("font_hover_color", COLOR_ACCENT_CORRUPT.lightened(0.2))
			btn.pressed.connect(_on_overwrite_requested.bind(slot_index))
	else:
		if _mode == "new":
			btn.text = "Начать здесь"
			btn.add_theme_color_override("font_color", COLOR_ACCENT)
			btn.add_theme_color_override("font_hover_color", COLOR_ACCENT_HOVER)
			btn.pressed.connect(_on_new_game_pressed.bind(slot_index))
		else:
			btn.text = "Нет данных"
			btn.disabled = true
			btn.add_theme_color_override("font_color", COLOR_TEXT_FAINT)

	return btn


# ============================================
# Диалог подтверждения перезаписи
# ============================================

func _build_confirm_dialog() -> void:
	var panel := PanelContainer.new()
	panel.set_anchors_and_offsets_preset(Control.PRESET_CENTER)
	panel.custom_minimum_size = Vector2(360, 160)
	panel.visible = false
	panel.add_theme_stylebox_override("panel", _make_panel_style(false, COLOR_ACCENT_CORRUPT))

	var box := VBoxContainer.new()
	box.alignment = BoxContainer.ALIGNMENT_CENTER
	box.add_theme_constant_override("separation", 16)
	panel.add_child(box)

	# Текст предупреждения
	var warning := Label.new()
	warning.text = "Стереть память об этом походе и начать заново?"
	warning.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	warning.autowrap_mode = TextServer.AUTOWRAP_WORD
	warning.add_theme_color_override("font_color", COLOR_TEXT_DIM)
	box.add_child(warning)

	# Кнопки
	var btn_row := HBoxContainer.new()
	btn_row.alignment = BoxContainer.ALIGNMENT_CENTER
	btn_row.add_theme_constant_override("separation", 20)
	box.add_child(btn_row)

	var yes_btn := _make_text_button("Да", _on_confirm_overwrite)
	yes_btn.add_theme_color_override("font_color", COLOR_ACCENT)
	btn_row.add_child(yes_btn)

	var no_btn := _make_text_button("Отмена", _on_cancel_overwrite)
	btn_row.add_child(no_btn)

	_confirm_popup = panel
	add_child(panel)


# ============================================
# Фабрики UI-элементов
# ============================================

func _make_label(text: String, size: int, color: Color) -> Label:
	var lbl := Label.new()
	lbl.text = text
	lbl.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	lbl.add_theme_font_size_override("font_size", size)
	lbl.add_theme_color_override("font_color", color)
	return lbl


func _make_text_button(text: String, callback: Callable) -> Button:
	var btn := Button.new()
	btn.text = text
	btn.flat = true
	btn.custom_minimum_size = Vector2(100, 36)
	btn.add_theme_font_size_override("font_size", 18)
	btn.add_theme_color_override("font_color", COLOR_TEXT_DIM)
	btn.add_theme_color_override("font_hover_color", COLOR_ACCENT)
	btn.pressed.connect(callback)
	return btn


func _make_panel_style(filled: bool, color: Color = COLOR_BORDER_EMPTY) -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = COLOR_PANEL_BG
	style.border_color = COLOR_BORDER_FILLED if filled else color
	style.set_border_width_all(2)
	style.set_corner_radius_all(4)  # Чуть больше скругление
	return style


# ============================================
# Стили кнопок действий
# ============================================

func _button_style_normal() -> StyleBoxFlat:
	var s := StyleBoxFlat.new()
	s.bg_color = COLOR_BUTTON_BG
	s.border_color = COLOR_BUTTON_BORDER
	s.set_border_width_all(1)
	s.set_corner_radius_all(4)
	s.content_margin_left = 16
	s.content_margin_right = 16
	return s


func _button_style_hover() -> StyleBoxFlat:
	var s := _button_style_normal()
	s.bg_color = Color(0.12, 0.10, 0.14, 0.8)
	s.border_color = COLOR_ACCENT
	return s


func _button_style_pressed() -> StyleBoxFlat:
	var s := _button_style_normal()
	s.bg_color = Color(0.06, 0.05, 0.08, 0.9)
	s.border_color = COLOR_ACCENT.darkened(0.3)
	return s


# ============================================
# Обработчики действий
# ============================================

func _on_back_pressed() -> void:
	SceneManager.change_scene(PATH_MAIN_MENU)


func _on_new_game_pressed(slot_index: int) -> void:
	SaveSystem.create_new_save(slot_index)
	_start_game(slot_index)


func _on_continue_pressed(slot_index: int) -> void:
	_start_game(slot_index)


func _on_overwrite_requested(slot_index: int) -> void:
	_pending_overwrite_slot = slot_index
	_confirm_popup.visible = true


func _on_confirm_overwrite() -> void:
	_confirm_popup.visible = false
	if _pending_overwrite_slot >= 0:
		_on_new_game_pressed(_pending_overwrite_slot)
	_pending_overwrite_slot = -1


func _on_cancel_overwrite() -> void:
	_confirm_popup.visible = false
	_pending_overwrite_slot = -1


func _start_game(slot_index: int) -> void:
	get_tree().set_meta("active_save_slot", slot_index)
	SceneManager.change_scene(PATH_GAMEPLAY)