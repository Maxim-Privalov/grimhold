extends Control


@export var grid_step: int = 56
@export var crack_color: Color = Color(0.10, 0.07, 0.09, 0.22)
@export var base_color_top: Color = Color(0.035, 0.025, 0.03, 1.0)
@export var base_color_bottom: Color = Color(0.06, 0.02, 0.04, 1.0)
@export var corruption_color: Color = Color(0.45, 0.10, 0.55, 0.10)
@export var ember_color: Color = Color(0.85, 0.35, 0.10, 0.9)

var _time: float = 0.0
var _embers: Array = []
const EMBER_COUNT := 22

func _ready() -> void:
	set_anchors_preset(Control.PRESET_FULL_RECT)
	mouse_filter = Control.MOUSE_FILTER_IGNORE
	randomize()
	for i in range(EMBER_COUNT):
		_embers.append(_make_ember())

func _make_ember() -> Dictionary:
	return {
		"x": randf(),
		"y": randf_range(0.6, 1.2),
		"speed": randf_range(8.0, 22.0),
		"drift": randf_range(-6.0, 6.0),
		"size": randf_range(1.0, 2.6),
		"flicker": randf_range(0.0, TAU),
	}

func _process(delta: float) -> void:
	_time += delta
	for e in _embers:
		e["y"] -= (e["speed"] * delta) / max(size.y, 1.0)
		e["x"] += (e["drift"] * delta) / max(size.x, 1.0)
		if e["y"] < -0.05:
			var ne := _make_ember()
			ne["y"] = 1.05
			e["x"] = ne["x"]; e["y"] = ne["y"]; e["speed"] = ne["speed"]
			e["drift"] = ne["drift"]; e["size"] = ne["size"]; e["flicker"] = ne["flicker"]
	queue_redraw()

func _draw() -> void:
	var steps := 24
	for i in range(steps):
		var t0 := float(i) / steps
		var t1 := float(i + 1) / steps
		var c := base_color_top.lerp(base_color_bottom, t0)
		draw_rect(Rect2(0, size.y * t0, size.x, size.y * (t1 - t0) + 1.0), c, true)

	var x := 0.0
	var seed_offset := 0
	while x < size.x:
		var jitter := sin(x * 0.013 + 1.7) * 18.0
		draw_line(Vector2(x + jitter, 0), Vector2(x - jitter, size.y), crack_color, 1.0)
		x += grid_step
		seed_offset += 1
	var y := 0.0
	while y < size.y:
		var jitter2 := cos(y * 0.011 + 0.4) * 14.0
		draw_line(Vector2(0, y + jitter2), Vector2(size.x, y - jitter2), crack_color, 1.0)
		y += grid_step

	var pulse := (sin(_time * 0.8) + 1.0) * 0.5
	var corruption := Color(corruption_color, corruption_color.a * (0.5 + pulse * 0.6))
	draw_rect(Rect2(0, size.y - 220, size.x, 220), corruption, true)
	draw_rect(Rect2(0, 0, size.x, 4.0), Color(corruption_color, 0.15 + pulse * 0.1), true)

	for e in _embers:
		var flick := 0.5 + 0.5 * sin(_time * 4.0 + e["flicker"])
		var c := Color(ember_color, ember_color.a * flick)
		var pos := Vector2(e["x"] * size.x, e["y"] * size.y)
		draw_circle(pos, e["size"], c)

	var vignette := Color(0, 0, 0, 0.55)
	var edge := 160.0
	draw_rect(Rect2(0, 0, size.x, edge), Color(vignette, 0.3), true)
	draw_rect(Rect2(0, size.y - edge, size.x, edge), Color(vignette, 0.35), true)
	draw_rect(Rect2(0, 0, edge * 0.6, size.y), Color(vignette, 0.2), true)
	draw_rect(Rect2(size.x - edge * 0.6, 0, edge * 0.6, size.y), Color(vignette, 0.2), true)
