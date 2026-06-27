extends CanvasLayer
## Автозагрузка (Project Settings -> Autoload -> SceneManager)
## Даёт переход между сценами с fade-эффектом из любого места проекта.

var _fade_rect: ColorRect
var _is_busy := false

const FADE_TIME := 0.28

func _ready() -> void:
	layer = 128
	_fade_rect = ColorRect.new()
	_fade_rect.color = Color(0, 0, 0, 0)
	_fade_rect.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_fade_rect.set_anchors_preset(Control.PRESET_FULL_RECT)
	add_child(_fade_rect)


func change_scene(path: String) -> void:
	if _is_busy:
		return
	_is_busy = true
	_fade_rect.mouse_filter = Control.MOUSE_FILTER_STOP
	var tw := create_tween()
	tw.tween_property(_fade_rect, "color:a", 1.0, FADE_TIME)
	await tw.finished
	get_tree().change_scene_to_file(path)
	await get_tree().process_frame
	var tw2 := create_tween()
	tw2.tween_property(_fade_rect, "color:a", 0.0, FADE_TIME)
	await tw2.finished
	_fade_rect.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_is_busy = false


func quit_game() -> void:
	var tw := create_tween()
	tw.tween_property(_fade_rect, "color:a", 1.0, FADE_TIME)
	await tw.finished
	get_tree().quit()
