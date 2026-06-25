extends Node
## Автозагрузка (Project Settings -> Autoload -> SaveSystem)
## Хранит и читает данные двух слотов сохранения.

const SAVE_DIR := "user://saves/"
const SLOT_COUNT := 2

func _ready() -> void:
	if not DirAccess.dir_exists_absolute(SAVE_DIR):
		DirAccess.make_dir_recursive_absolute(SAVE_DIR)


func _slot_path(slot_index: int) -> String:
	return "%sslot_%d.save" % [SAVE_DIR, slot_index]


func has_save(slot_index: int) -> bool:
	return FileAccess.file_exists(_slot_path(slot_index))


## Возвращает Dictionary с данными слота или null, если слот пуст.
func get_slot_info(slot_index: int) -> Variant:
	if not has_save(slot_index):
		return null
	var f := FileAccess.open(_slot_path(slot_index), FileAccess.READ)
	if f == null:
		return null
	var text := f.get_as_text()
	f.close()
	var json := JSON.new()
	if json.parse(text) != OK:
		return null
	return json.data


## Создаёт новое сохранение в указанном слоте (вызывается при "Новая игра").
func create_new_save(slot_index: int) -> void:
	var data := {
		"slot": slot_index,
		"created_at": Time.get_datetime_string_from_system()
	}
	save_game(slot_index, data)


func save_game(slot_index: int, data: Dictionary) -> void:
	var f := FileAccess.open(_slot_path(slot_index), FileAccess.WRITE)
	if f:
		f.store_string(JSON.stringify(data, "\t"))
		f.close()


func delete_save(slot_index: int) -> void:
	if has_save(slot_index):
		DirAccess.remove_absolute(_slot_path(slot_index))
