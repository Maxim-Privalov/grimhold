extends Control

const PATH_SAVE_SLOTS := "res://Scenes/UI/Menu/SaveSlotsMenu.tscn"
const PATH_NEW_GAME := "res://Scenes/World/Levels/LevelOne.tscn"
const PATH_SETTINGS := "res://Scenes/UI/Menu/SettingsMenu.tscn"

const COLOR_ACCENT := Color(0.82, 0.30, 0.18)
const COLOR_ACCENT_HOVER := Color(0.95, 0.45, 0.25)
const COLOR_ACCENT_CORRUPT := Color(0.55, 0.20, 0.65)
const COLOR_TEXT := Color(0.85, 0.82, 0.78)
const COLOR_TEXT_DIM := Color(0.62, 0.56, 0.52)
const COLOR_TEXT_FAINT := Color(0.40, 0.34, 0.34)
const COLOR_BG_OVERLAY := Color(0.02, 0.02, 0.04, 0.55)
const COLOR_BUTTON_BG := Color(0.08, 0.07, 0.09, 0.65)
const COLOR_BUTTON_BORDER := Color(0.25, 0.22, 0.25, 0.4)

var _buttons: Array[Button] = []

func _ready() -> void:
	set_anchors_preset(Control.PRESET_FULL_RECT)
	_build_ui()
	_setup_shortcuts()


func _build_ui() -> void:
	# === Фоновый слой ===
	var bg_image := TextureRect.new()
	bg_image.set_anchors_preset(Control.PRESET_FULL_RECT)
	bg_image.texture = preload("res://Assets/Backgrounds/BackgroundMain.png")
	bg_image.expand_mode = TextureRect.EXPAND_IGNORE_SIZE
	bg_image.stretch_mode = TextureRect.STRETCH_SCALE
	add_child(bg_image)

	var overlay := ColorRect.new()
	overlay.set_anchors_preset(Control.PRESET_FULL_RECT)
	overlay.color = COLOR_BG_OVERLAY
	add_child(overlay)

	# === Декоративная рамка (по желанию) ===
	var border := PanelContainer.new()
	border.set_anchors_preset(Control.PRESET_FULL_RECT)
	var border_style := StyleBoxFlat.new()
	border_style.bg_color = Color.TRANSPARENT
	border_style.border_width_left = 2
	border_style.border_width_right = 2
	border_style.border_color = Color(0.3, 0.25, 0.3, 0.2)
	border_style.content_margin_left = 2
	border_style.content_margin_right = 2
	border.add_theme_stylebox_override("panel", border_style)
	add_child(border)

	# === Основной контейнер ===
	var root := VBoxContainer.new()
	root.alignment = BoxContainer.ALIGNMENT_CENTER
	root.add_theme_constant_override("separation", 22)
	root.set_anchors_preset(Control.PRESET_FULL_RECT)
	border.add_child(root)

	# Растягивающийся спейсер сверху
	var spacer_top := Control.new()
	spacer_top.size_flags_vertical = Control.SIZE_EXPAND_FILL
	root.add_child(spacer_top)

	# === Логотип ===
	var title_wrap := VBoxContainer.new()
	title_wrap.alignment = BoxContainer.ALIGNMENT_CENTER
	title_wrap.size_flags_horizontal = Control.SIZE_SHRINK_CENTER
	root.add_child(title_wrap)

	var title := TextureRect.new()
	title.texture = preload("res://Assets/Others/titleLogo.png")
	title.expand_mode = TextureRect.EXPAND_IGNORE_SIZE
	title.stretch_mode = TextureRect.STRETCH_KEEP_ASPECT_CENTERED
	title.custom_minimum_size = Vector2(title.texture.get_width() / 2.5, title.texture.get_height() / 2.5)
	title_wrap.add_child(title)

	# Отступ после логотипа
	var gap_after_logo := Control.new()
	gap_after_logo.custom_minimum_size = Vector2(0, 20)
	root.add_child(gap_after_logo)

	# === Меню с кнопками ===
	var menu_box := VBoxContainer.new()
	menu_box.alignment = BoxContainer.ALIGNMENT_CENTER
	menu_box.add_theme_constant_override("separation", 10)
	menu_box.custom_minimum_size = Vector2(340, 0)
	menu_box.size_flags_horizontal = Control.SIZE_SHRINK_CENTER
	root.add_child(menu_box)

	_add_menu_button(menu_box, "Новая игра", _on_new_game_pressed, false)
	_add_menu_button(menu_box, "Продолжить", _on_continue_pressed, false)
	_add_menu_button(menu_box, "Настройки", _on_settings_pressed, false)
	_add_menu_button(menu_box, "Выйти", _on_quit_pressed, false)

	# Растягивающийся спейсер снизу
	var spacer_bottom := Control.new()
	spacer_bottom.size_flags_vertical = Control.SIZE_EXPAND_FILL
	root.add_child(spacer_bottom)

	# === Версия ===
	var version := Label.new()
	version.text = "build 0.1.0"
	version.add_theme_font_size_override("font_size", 20)
	version.add_theme_color_override("font_color", Color(COLOR_TEXT_FAINT, 0.7))
	version.horizontal_alignment = HORIZONTAL_ALIGNMENT_RIGHT
	version.vertical_alignment = VERTICAL_ALIGNMENT_BOTTOM

	# Привязываем к правому нижнему углу
	version.anchor_left = 1.0
	version.anchor_right = 1.0
	version.anchor_top = 1.0
	version.anchor_bottom = 1.0
	version.offset_left = -160  # ширина области
	version.offset_right = -16  # отступ справа
	version.offset_top = -32    # высота + отступ снизу
	version.offset_bottom = -8  # отступ снизу

	add_child(version)  # Добавляем напрямую, а не в root

	# Дополнительный отступ снизу
	var bottom_padding := Control.new()
	bottom_padding.custom_minimum_size = Vector2(0, 24)
	root.add_child(bottom_padding)


func _add_menu_button(parent: Control, text: String, callback: Callable, is_primary: bool) -> void:
	var btn := Button.new()
	btn.text = text
	btn.custom_minimum_size = Vector2(0, 50)
	btn.size_flags_horizontal = Control.SIZE_FILL
	btn.size_flags_vertical = Control.SIZE_SHRINK_CENTER
	
	# Шрифт
	btn.add_theme_font_size_override("font_size", 18)
	btn.add_theme_color_override("font_color", COLOR_TEXT if is_primary else COLOR_TEXT_DIM)
	btn.add_theme_color_override("font_hover_color", COLOR_ACCENT_HOVER)
	btn.add_theme_color_override("font_pressed_color", COLOR_ACCENT)
	btn.add_theme_color_override("font_focus_color", COLOR_ACCENT_HOVER)
	
	# Выравнивание текста — по центру
	btn.alignment = HORIZONTAL_ALIGNMENT_CENTER
	
	# Плоский режим и стили
	btn.flat = false
	
	# Нормальное состояние
	var style_normal := StyleBoxFlat.new()
	style_normal.bg_color = COLOR_BUTTON_BG
	style_normal.border_width_left = 1
	style_normal.border_width_right = 1
	style_normal.border_width_top = 1
	style_normal.border_width_bottom = 1
	style_normal.border_color = COLOR_BUTTON_BORDER
	style_normal.corner_radius_top_left = 4
	style_normal.corner_radius_top_right = 4
	style_normal.corner_radius_bottom_left = 4
	style_normal.corner_radius_bottom_right = 4
	style_normal.content_margin_left = 24
	style_normal.content_margin_right = 24
	btn.add_theme_stylebox_override("normal", style_normal)
	
	# Состояние наведения
	var style_hover := style_normal.duplicate()
	style_hover.bg_color = Color(0.12, 0.10, 0.14, 0.8)
	style_hover.border_color = COLOR_ACCENT
	btn.add_theme_stylebox_override("hover", style_hover)
	
	# Состояние нажатия
	var style_pressed := style_normal.duplicate()
	style_pressed.bg_color = Color(0.06, 0.05, 0.08, 0.9)
	style_pressed.border_color = COLOR_ACCENT.darkened(0.3)
	btn.add_theme_stylebox_override("pressed", style_pressed)
	
	# Состояние фокуса
	var style_focus := style_normal.duplicate()
	style_focus.border_color = Color(COLOR_ACCENT, 0.6)
	btn.add_theme_stylebox_override("focus", style_focus)
	
	btn.focus_mode = Control.FOCUS_ALL
	
	parent.add_child(btn)
	btn.pressed.connect(callback)
		
	_buttons.append(btn)



func _setup_shortcuts() -> void:
	# Горячие клавиши
	if not InputMap.has_action("ui_accept"):
		return
	
	# Первая кнопка получает фокус автоматически
	if _buttons.size() > 0:
		_buttons[0].grab_focus()


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
