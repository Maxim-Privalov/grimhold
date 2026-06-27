using Godot;
using System;
using System.Collections.Generic;

public partial class SaveSystem : Node
{
    private const string SAVE_DIR = "user://saves/";
    private const int SLOT_COUNT = 2;

    public override void _Ready()
    {
        // Создаем директорию для сохранений, если её нет
        if (!DirAccess.DirExistsAbsolute(SAVE_DIR))
        {
            var error = DirAccess.MakeDirRecursiveAbsolute(SAVE_DIR);
            if (error != Error.Ok)
            {
                GD.PrintErr($"Не удалось создать директорию сохранений: {SAVE_DIR}");
            }
            else
            {
                GD.Print($"Директория сохранений создана: {SAVE_DIR}");
            }
        }
    }

    /// <summary>
    /// Возвращает путь к файлу сохранения для указанного слота
    /// </summary>
    private string GetSlotPath(int slotIndex)
    {
        return $"{SAVE_DIR}slot_{slotIndex}.save";
    }

    /// <summary>
    /// Проверяет, существует ли сохранение в указанном слоте
    /// </summary>
    public bool HasSave(int slotIndex)
    {
        return FileAccess.FileExists(GetSlotPath(slotIndex));
    }

    /// <summary>
    /// Возвращает Dictionary с данными слота или null, если слот пуст
    /// </summary>
    public Godot.Collections.Dictionary GetSlotInfo(int slotIndex)
    {
        if (!HasSave(slotIndex))
            return null;

        using var file = FileAccess.Open(GetSlotPath(slotIndex), FileAccess.ModeFlags.Read);
        if (file == null)
        {
            GD.PrintErr($"Не удалось открыть файл сохранения слота {slotIndex}");
            return null;
        }

        string text = file.GetAsText();
        
        var json = new Json();
        var error = json.Parse(text);
        
        if (error != Error.Ok)
        {
            GD.PrintErr($"Ошибка парсинга JSON в слоте {slotIndex}: {error}");
            return null;
        }

        // json.Data возвращает Variant, преобразуем в Dictionary
        var data = json.Data;
        if (data.VariantType == Variant.Type.Dictionary)
        {
            return data.AsGodotDictionary();
        }
        
        GD.PrintErr($"Данные в слоте {slotIndex} не являются Dictionary");
        return null;
    }

    /// <summary>
    /// Создаёт новое сохранение в указанном слоте (вызывается при "Новая игра")
    /// </summary>
    public void CreateNewSave(int slotIndex)
    {
        var data = new Godot.Collections.Dictionary
        {
            { "slot", slotIndex },
            { "created_at", Time.GetDatetimeStringFromSystem() }
        };
        
        SaveGame(slotIndex, data);
        GD.Print($"Создано новое сохранение в слоте {slotIndex}");
    }

    /// <summary>
    /// Сохраняет данные в указанный слот
    /// </summary>
    public void SaveGame(int slotIndex, Godot.Collections.Dictionary data)
    {
        using var file = FileAccess.Open(GetSlotPath(slotIndex), FileAccess.ModeFlags.Write);
        if (file == null)
        {
            GD.PrintErr($"Не удалось создать файл сохранения для слота {slotIndex}");
            return;
        }

        string jsonString = Json.Stringify(data, "\t");
        file.StoreString(jsonString);
        
        GD.Print($"Сохранение в слот {slotIndex} успешно записано");
    }

    /// <summary>
    /// Удаляет сохранение из указанного слота
    /// </summary>
    public void DeleteSave(int slotIndex)
    {
        if (HasSave(slotIndex))
        {
            var error = DirAccess.RemoveAbsolute(GetSlotPath(slotIndex));
            if (error == Error.Ok)
            {
                GD.Print($"Сохранение в слоте {slotIndex} удалено");
            }
            else
            {
                GD.PrintErr($"Ошибка удаления сохранения в слоте {slotIndex}: {error}");
            }
        }
        else
        {
            GD.Print($"Сохранение в слоте {slotIndex} не существует");
        }
    }

    /// <summary>
    /// Возвращает информацию о всех слотах сохранения
    /// </summary>
    public Godot.Collections.Array<Godot.Collections.Dictionary> GetAllSlotsInfo()
    {
        var slots = new Godot.Collections.Array<Godot.Collections.Dictionary>();
        
        for (int i = 0; i < SLOT_COUNT; i++)
        {
            var info = GetSlotInfo(i);
            if (info != null)
            {
                slots.Add(info);
            }
        }
        
        return slots;
    }

    /// <summary>
    /// Проверяет, есть ли свободные слоты
    /// </summary>
    public bool HasEmptySlot()
    {
        for (int i = 0; i < SLOT_COUNT; i++)
        {
            if (!HasSave(i))
                return true;
        }
        return false;
    }

    /// <summary>
    /// Возвращает количество занятых слотов
    /// </summary>
    public int GetUsedSlotsCount()
    {
        int count = 0;
        for (int i = 0; i < SLOT_COUNT; i++)
        {
            if (HasSave(i))
                count++;
        }
        return count;
    }
}