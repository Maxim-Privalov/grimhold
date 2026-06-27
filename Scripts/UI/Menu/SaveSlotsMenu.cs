using Godot;
using System;
using System.Collections.Generic;

public partial class SaveSlotsMenu : Control
{
	// ============================================
	// Пути к сценам
	// ============================================
	private const string PATH_MAIN_MENU = "res://Scenes/UI/Menu/MainMenu.tscn";
	private const string PATH_GAME = "res://Scenes/World/Game.tscn";

	// ============================================
	// Цветовая палитра (единая точка правды)
	// ============================================
	private static readonly Color COLOR_ACCENT = new Color(0.82f, 0.30f, 0.18f);
	private static readonly Color COLOR_ACCENT_HOVER = new Color(0.95f, 0.45f, 0.25f);
	private static readonly Color COLOR_ACCENT_CORRUPT = new Color(0.55f, 0.20f, 0.65f);
	private static readonly Color COLOR_TEXT = new Color(0.85f, 0.82f, 0.78f);
	private static readonly Color COLOR_TEXT_DIM = new Color(0.62f, 0.56f, 0.52f);
	private static readonly Color COLOR_TEXT_FAINT = new Color(0.40f, 0.34f, 0.34f);
	private static readonly Color COLOR_PANEL_BG = new Color(0.05f, 0.035f, 0.04f, 0.92f);
	private static readonly Color COLOR_BORDER_EMPTY = new Color(0.22f, 0.18f, 0.18f);
	private static readonly Color COLOR_BORDER_FILLED = COLOR_ACCENT;
	private static readonly Color COLOR_OVERLAY = new Color(0.02f, 0.02f, 0.04f, 0.55f);
	private static readonly Color COLOR_BUTTON_BG = new Color(0.08f, 0.07f, 0.09f, 0.65f);
	private static readonly Color COLOR_BUTTON_BORDER = new Color(0.25f, 0.22f, 0.25f, 0.4f);

	// ============================================
	// Параметры
	// ============================================
	private const int SLOT_COUNT = 2;

	// ============================================
	// Состояние
	// ============================================
	private string _mode = "continue";  // "continue" | "new"
	private PanelContainer _confirmPopup;
	private int _pendingOverwriteSlot = -1;

	// Ссылки на Autoload
	private SceneManager _sceneManager;
	private SaveSystem _saveSystem;

	// ============================================
	// Жизненный цикл
	// ============================================

	public override void _Ready()
	{
		// Получаем синглтоны
		_sceneManager = GetNode<SceneManager>("/root/SceneManager");
		_saveSystem = GetNode<SaveSystem>("/root/SaveSystem");

		SetAnchorsPreset(LayoutPreset.FullRect);
		
		// Получаем режим из метаданных
		_mode = GetTree().GetMeta("save_slots_mode", "continue").AsString();
		
		BuildBackground();
		BuildContent();
		BuildConfirmDialog();
	}

	// ============================================
	// Построение UI
	// ============================================

	private void BuildBackground()
	{
		// Фоновое изображение
		var bg = new TextureRect();
		bg.SetAnchorsPreset(LayoutPreset.FullRect);
		bg.Texture = GD.Load<Texture2D>("res://Assets/Backgrounds/BackgroundMain.png");
		bg.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
		bg.StretchMode = TextureRect.StretchModeEnum.Scale;
		AddChild(bg);

		// Затемняющий оверлей
		var overlay = new ColorRect();
		overlay.SetAnchorsPreset(LayoutPreset.FullRect);
		overlay.Color = COLOR_OVERLAY;
		AddChild(overlay);
	}

	private void BuildContent()
	{
		var root = new VBoxContainer();
		root.Alignment = BoxContainer.AlignmentMode.Center;
		root.SetAnchorsPreset(LayoutPreset.FullRect);
		AddChild(root);

		// === Верхняя панель: назад / заголовок / спейсер ===
		root.AddChild(BuildTopBar());

		// === Центральная область со слотами ===
		var center = new CenterContainer();
		center.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
		center.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
		root.AddChild(center);

		var slotsRow = new HBoxContainer();
		slotsRow.AddThemeConstantOverride("separation", 32);
		center.AddChild(slotsRow);

		for (int i = 0; i < SLOT_COUNT; i++)
		{
			slotsRow.AddChild(CreateSlotPanel(i));
		}
	}

	private Control BuildTopBar()
	{
		var bar = new HBoxContainer();
		bar.CustomMinimumSize = new Vector2(0, 64);

		// Кнопка "Назад"
		var backBtn = MakeTextButton("< Назад", () => OnBackPressed());
		backBtn.CustomMinimumSize = new Vector2(140, 0);
		bar.AddChild(backBtn);

		// Заголовок
		var title = new Label();
		title.Text = _mode == "new" ? "НОВЫЙ ПОХОД" : "ПРОДОЛЖИТЬ";
		title.AddThemeFontSizeOverride("font_size", 26);
		title.AddThemeColorOverride("font_color", COLOR_TEXT_DIM);
		title.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
		title.HorizontalAlignment = HorizontalAlignment.Center;
		bar.AddChild(title);

		// Спейсер для баланса
		var spacer = new Control();
		spacer.CustomMinimumSize = new Vector2(140, 0);
		bar.AddChild(spacer);

		return bar;
	}

	private Control CreateSlotPanel(int slotIndex)
	{
		var saveData = _saveSystem.GetSlotInfo(slotIndex);
		bool isEmpty = saveData == null;

		// --- Контейнер слота ---
		var panel = new PanelContainer();
		panel.CustomMinimumSize = new Vector2(300, 360);
		panel.AddThemeStyleboxOverride("panel", MakePanelStyle(!isEmpty));

		// --- Внутренний вертикальный контейнер ---
		var inner = new VBoxContainer();
		inner.Alignment = BoxContainer.AlignmentMode.Center;
		inner.AddThemeConstantOverride("separation", 10);
		panel.AddChild(inner);

		// Заголовок слота
		inner.AddChild(MakeLabel(
			$"СЛОТ {slotIndex + 1}",
			16, COLOR_TEXT_DIM
		));

		// Иконка
		var icon = MakeLabel(
			!isEmpty ? "✠" : "✦",
			40,
			!isEmpty ? COLOR_ACCENT : COLOR_BORDER_EMPTY
		);
		inner.AddChild(icon);

		// Информация о сохранении
		var body = MakeLabel("", 14, COLOR_TEXT_DIM);
		body.AutowrapMode = TextServer.AutowrapMode.Word;
		
		if (!isEmpty)
		{
			string createdAt = saveData.ContainsKey("created_at") 
				? saveData["created_at"].AsString() 
				: "???";
			body.Text = $"Глава ?\n{createdAt}";
		}
		else
		{
			body.Text = "Тлен и пустота";
			body.AddThemeColorOverride("font_color", new Color(0.4f, 0.36f, 0.34f));
		}
		inner.AddChild(body);

		// Кнопка действия
		inner.AddChild(MakeActionButton(slotIndex, isEmpty));

		return panel;
	}

	private Button MakeActionButton(int slotIndex, bool isEmpty)
	{
		var btn = new Button();
		btn.CustomMinimumSize = new Vector2(180, 40);
		btn.Flat = false;
		btn.Alignment = HorizontalAlignment.Center;
		btn.AddThemeFontSizeOverride("font_size", 16);

		// Стили (общие)
		btn.AddThemeStyleboxOverride("normal", ButtonStyleNormal());
		btn.AddThemeStyleboxOverride("hover", ButtonStyleHover());
		btn.AddThemeStyleboxOverride("pressed", ButtonStylePressed());

		if (!isEmpty)
		{
			if (_mode == "continue")
			{
				btn.Text = "Продолжить";
				btn.AddThemeColorOverride("font_color", COLOR_ACCENT);
				btn.AddThemeColorOverride("font_hover_color", COLOR_ACCENT_HOVER);
				int capturedSlot = slotIndex; // Захватываем переменную для лямбды
				btn.Pressed += () => OnContinuePressed(capturedSlot);
			}
			else
			{
				btn.Text = "Перезаписать";
				btn.AddThemeColorOverride("font_color", COLOR_ACCENT_CORRUPT);
				btn.AddThemeColorOverride("font_hover_color", COLOR_ACCENT_CORRUPT.Lightened(0.2f));
				int capturedSlot = slotIndex;
				btn.Pressed += () => OnOverwriteRequested(capturedSlot);
			}
		}
		else
		{
			if (_mode == "new")
			{
				btn.Text = "Начать здесь";
				btn.AddThemeColorOverride("font_color", COLOR_ACCENT);
				btn.AddThemeColorOverride("font_hover_color", COLOR_ACCENT_HOVER);
				int capturedSlot = slotIndex;
				btn.Pressed += () => OnNewGamePressed(capturedSlot);
			}
			else
			{
				btn.Text = "Нет данных";
				btn.Disabled = true;
				btn.AddThemeColorOverride("font_color", COLOR_TEXT_FAINT);
			}
		}

		return btn;
	}

	// ============================================
	// Диалог подтверждения перезаписи
	// ============================================

	private async void BuildConfirmDialog()
	{
		var panel = new PanelContainer();
		panel.SetAnchorsAndOffsetsPreset(LayoutPreset.Center);
		panel.CustomMinimumSize = new Vector2(360, 160);
		panel.Visible = false;
		panel.AddThemeStyleboxOverride("panel", MakePanelStyle(false, COLOR_ACCENT_CORRUPT));

		var box = new VBoxContainer();
		box.Alignment = BoxContainer.AlignmentMode.Center;
		box.AddThemeConstantOverride("separation", 16);
		panel.AddChild(box);

		// Текст предупреждения
		var warning = new Label();
		warning.Text = "Стереть память об этом походе и начать заново?";
		warning.HorizontalAlignment = HorizontalAlignment.Center;
		warning.AutowrapMode = TextServer.AutowrapMode.Word;
		warning.AddThemeColorOverride("font_color", COLOR_TEXT_DIM);
		box.AddChild(warning);

		// Кнопки
		var btnRow = new HBoxContainer();
		btnRow.Alignment = BoxContainer.AlignmentMode.Center;
		btnRow.AddThemeConstantOverride("separation", 20);
		box.AddChild(btnRow);

		var yesBtn = MakeTextButton("Да", () => OnConfirmOverwrite());
		yesBtn.AddThemeColorOverride("font_color", COLOR_ACCENT);
		btnRow.AddChild(yesBtn);

		var noBtn = MakeTextButton("Отмена", () => OnCancelOverwrite());
		btnRow.AddChild(noBtn);

		_confirmPopup = panel;
		AddChild(panel);

		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

		panel.OffsetLeft -= panel.Size.X / 2;
		panel.OffsetTop -= panel.Size.Y / 2;
		panel.OffsetRight -= panel.Size.X / 2;
		panel.OffsetBottom -= panel.Size.Y / 2;
	}

	// ============================================
	// Фабрики UI-элементов
	// ============================================

	private Label MakeLabel(string text, int size, Color color)
	{
		var lbl = new Label();
		lbl.Text = text;
		lbl.HorizontalAlignment = HorizontalAlignment.Center;
		lbl.AddThemeFontSizeOverride("font_size", size);
		lbl.AddThemeColorOverride("font_color", color);
		return lbl;
	}

	private Button MakeTextButton(string text, Action callback)
	{
		var btn = new Button();
		btn.Text = text;
		btn.Flat = true;
		btn.CustomMinimumSize = new Vector2(100, 36);
		btn.AddThemeFontSizeOverride("font_size", 18);
		btn.AddThemeColorOverride("font_color", COLOR_TEXT_DIM);
		btn.AddThemeColorOverride("font_hover_color", COLOR_ACCENT);
		btn.Pressed += callback;
		return btn;
	}

	private StyleBoxFlat MakePanelStyle(bool filled, Color? color = null)
	{
		Color borderColor = color ?? COLOR_BORDER_EMPTY;
		
		var style = new StyleBoxFlat();
		style.BgColor = COLOR_PANEL_BG;
		style.BorderColor = filled ? COLOR_BORDER_FILLED : borderColor;
		style.BorderWidthLeft = 2;
		style.BorderWidthRight = 2;
		style.BorderWidthTop = 2;
		style.BorderWidthBottom = 2;
		style.CornerRadiusTopLeft = 4;
		style.CornerRadiusTopRight = 4;
		style.CornerRadiusBottomLeft = 4;
		style.CornerRadiusBottomRight = 4;
		return style;
	}

	// ============================================
	// Стили кнопок действий
	// ============================================

	private StyleBoxFlat ButtonStyleNormal()
	{
		var s = new StyleBoxFlat();
		s.BgColor = COLOR_BUTTON_BG;
		s.BorderColor = COLOR_BUTTON_BORDER;
		s.BorderWidthLeft = 1;
		s.BorderWidthRight = 1;
		s.BorderWidthTop = 1;
		s.BorderWidthBottom = 1;
		s.CornerRadiusTopLeft = 4;
		s.CornerRadiusTopRight = 4;
		s.CornerRadiusBottomLeft = 4;
		s.CornerRadiusBottomRight = 4;
		s.ContentMarginLeft = 16;
		s.ContentMarginRight = 16;
		return s;
	}

	private StyleBoxFlat ButtonStyleHover()
	{
		var s = ButtonStyleNormal();
		s.BgColor = new Color(0.12f, 0.10f, 0.14f, 0.8f);
		s.BorderColor = COLOR_ACCENT;
		return s;
	}

	private StyleBoxFlat ButtonStylePressed()
	{
		var s = ButtonStyleNormal();
		s.BgColor = new Color(0.06f, 0.05f, 0.08f, 0.9f);
		s.BorderColor = COLOR_ACCENT.Darkened(0.3f);
		return s;
	}

	// ============================================
	// Обработчики действий
	// ============================================

	private void OnBackPressed()
	{
		_sceneManager?.ChangeScene(PATH_MAIN_MENU);
	}

	private void OnNewGamePressed(int slotIndex)
	{
		_saveSystem?.CreateNewSave(slotIndex);
		StartGame(slotIndex);
	}

	private void OnContinuePressed(int slotIndex)
	{
		StartGame(slotIndex);
	}

	private void OnOverwriteRequested(int slotIndex)
	{
		_pendingOverwriteSlot = slotIndex;
		_confirmPopup.Visible = true;
	}

	private void OnConfirmOverwrite()
	{
		_confirmPopup.Visible = false;
		if (_pendingOverwriteSlot >= 0)
		{
			OnNewGamePressed(_pendingOverwriteSlot);
		}
		_pendingOverwriteSlot = -1;
	}

	private void OnCancelOverwrite()
	{
		_confirmPopup.Visible = false;
		_pendingOverwriteSlot = -1;
	}

	private void StartGame(int slotIndex)
	{
		GetTree().SetMeta("active_save_slot", slotIndex);
		_sceneManager?.ChangeScene(PATH_GAME);
	}
}
