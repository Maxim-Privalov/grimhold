using Godot;
using System;
using System.Collections.Generic;

public partial class EscMenu : CanvasLayer
{
	// Цветовые константы
	private static readonly Color COLOR_BG_OVERLAY = new Color(0.05f, 0.05f, 0.08f, 0.85f);
	private static readonly Color COLOR_TEXT = new Color(0.9f, 0.85f, 0.9f);
	private static readonly Color COLOR_TEXT_DIM = new Color(0.6f, 0.55f, 0.65f);
	private static readonly Color COLOR_TEXT_FAINT = new Color(0.4f, 0.35f, 0.45f);
	private static readonly Color COLOR_ACCENT = new Color(0.6f, 0.5f, 0.8f);
	private static readonly Color COLOR_ACCENT_HOVER = new Color(0.7f, 0.6f, 0.9f);
	private static readonly Color COLOR_ACCENT_DANGER = new Color(0.9f, 0.3f, 0.3f);
	private static readonly Color COLOR_ACCENT_DANGER_HOVER = new Color(1.0f, 0.4f, 0.4f);
	private static readonly Color COLOR_BUTTON_BG = new Color(0.08f, 0.07f, 0.1f, 0.7f);
	private static readonly Color COLOR_BUTTON_BORDER = new Color(0.2f, 0.18f, 0.25f, 0.5f);
	private static readonly Color COLOR_MODAL_BG = new Color(0.08f, 0.07f, 0.1f, 0.95f);
	private static readonly Color COLOR_MODAL_BORDER = new Color(0.6f, 0.5f, 0.8f, 0.5f);
	private static readonly Color COLOR_PANEL_BG = new Color(0.05f, 0.035f, 0.04f, 0.92f);
	private static readonly Color COLOR_BORDER_FAINT = new Color(0.22f, 0.18f, 0.18f);

	// Пути и константы
	private const string SCENE_MANAGER_PATH = "/root/SceneManager";
	private const string MAIN_MENU_SCENE_PATH = "res://Scenes/UI/Menu/MainMenu.tscn";
	private const string CONFIG_PATH = "user://settings.cfg";

	// Ссылки на синглтоны
	private SceneManager sceneManager = null;
	
	// UI элементы
	private List<Button> _buttons = new List<Button>();
	private bool _isPaused = false;
	
	// Основные контейнеры
	private VBoxContainer _mainMenuContainer;
	private VBoxContainer _settingsContainer;
	private Label _titleLabel;
	
	// Модальное окно выхода
	private PanelContainer _exitModalPanel;
	private Button _exitModalConfirmBtn;
	private Button _exitModalCancelBtn;
	
	// Настройки
	private ConfigFile _config = new ConfigFile();

	public override void _Ready()
	{
		Layer = 100;
		Visible = false;
		ProcessMode = ProcessModeEnum.Always;

		sceneManager = GetNode<SceneManager>(SCENE_MANAGER_PATH);
		
		// Загружаем конфигурацию
		var error = _config.Load(CONFIG_PATH);
		if (error != Error.Ok)
		{
			GD.Print($"Config file not found, using defaults. Error: {error}");
		}
		
		BuildUI();
		BuildSettingsUI();
		BuildExitModalDialog();
		SetupShortcuts();
	}

	public override void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed("ui_cancel"))
		{
			// Если открыто модальное окно - закрываем его
			if (_exitModalPanel.Visible)
			{
				HideExitModalDialog();
				return;
			}
			
			// Если настройки открыты - возвращаемся в главное меню
			if (_settingsContainer.Visible)
			{
				ShowMainMenu();
				return;
			}
			
			if (Visible)
				HideMenu();
			else
				ShowMenu();
		}
	}

	public void ShowMenu()
	{
		Visible = true;
		GetTree().Paused = true;
		_isPaused = true;
		
		ShowMainMenu();
	}

	public void HideMenu()
	{
		Visible = false;
		GetTree().Paused = false;
		_isPaused = false;
	}

	private void BuildUI()
	{
		// === Фоновое затемнение ===
		var overlay = new ColorRect();
		overlay.SetAnchorsPreset(Control.LayoutPreset.FullRect);
		overlay.Color = COLOR_BG_OVERLAY;
		overlay.MouseFilter = Control.MouseFilterEnum.Stop;
		AddChild(overlay);

		// === Декоративная рамка ===
		var border = new PanelContainer();
		border.SetAnchorsPreset(Control.LayoutPreset.FullRect);
		
		var borderStyle = new StyleBoxFlat();
		borderStyle.BgColor = Colors.Transparent;
		borderStyle.BorderWidthLeft = 2;
		borderStyle.BorderWidthRight = 2;
		borderStyle.BorderColor = new Color(0.3f, 0.25f, 0.3f, 0.2f);
		borderStyle.ContentMarginLeft = 2;
		borderStyle.ContentMarginRight = 2;
		border.AddThemeStyleboxOverride("panel", borderStyle);
		AddChild(border);

		// === Основной контейнер ===
		var root = new VBoxContainer();
		root.Alignment = BoxContainer.AlignmentMode.Center;
		root.AddThemeConstantOverride("separation", 22);
		root.SetAnchorsPreset(Control.LayoutPreset.FullRect);
		border.AddChild(root);

		// Растягивающийся спейсер сверху
		var spacerTop = new Control();
		spacerTop.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
		root.AddChild(spacerTop);

		// === Заголовок ===
		var titleWrap = new VBoxContainer();
		titleWrap.Alignment = BoxContainer.AlignmentMode.Center;
		titleWrap.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;
		root.AddChild(titleWrap);

		_titleLabel = new Label();
		_titleLabel.Text = "ПАУЗА";
		_titleLabel.AddThemeFontSizeOverride("font_size", 48);
		_titleLabel.AddThemeColorOverride("font_color", COLOR_ACCENT);
		_titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
		titleWrap.AddChild(_titleLabel);

		// Отступ после заголовка
		var gapAfterTitle = new Control();
		gapAfterTitle.CustomMinimumSize = new Vector2(0, 30);
		root.AddChild(gapAfterTitle);

		// === Главное меню ===
		_mainMenuContainer = new VBoxContainer();
		_mainMenuContainer.Alignment = BoxContainer.AlignmentMode.Center;
		_mainMenuContainer.AddThemeConstantOverride("separation", 10);
		_mainMenuContainer.CustomMinimumSize = new Vector2(340, 0);
		_mainMenuContainer.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;
		root.AddChild(_mainMenuContainer);

		AddMenuButton(_mainMenuContainer, "Продолжить", () => OnContinuePressed(), true);
		AddMenuButton(_mainMenuContainer, "Настройки", () => ShowSettingsMenu(), false);
		AddMenuButton(_mainMenuContainer, "Выйти в главное меню", () => OnExitToMainMenuPressed(), false);

		// === Контейнер настроек (изначально скрыт) ===
		_settingsContainer = new VBoxContainer();
		_settingsContainer.Alignment = BoxContainer.AlignmentMode.Center;
		_settingsContainer.AddThemeConstantOverride("separation", 10);
		_settingsContainer.CustomMinimumSize = new Vector2(480, 0);
		_settingsContainer.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;
		_settingsContainer.Visible = false;
		root.AddChild(_settingsContainer);

		// Растягивающийся спейсер снизу
		var spacerBottom = new Control();
		spacerBottom.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
		root.AddChild(spacerBottom);

		// === Информация внизу ===
		var infoLabel = new Label
		{
			Text = "ESC - закрыть меню",
			AnchorLeft = 0.0f,
			AnchorRight = 1.0f,
			AnchorTop = 1.0f,
			AnchorBottom = 1.0f,
			OffsetTop = -40,
			OffsetBottom = -16,
			HorizontalAlignment = HorizontalAlignment.Center
		};

		infoLabel.AddThemeFontSizeOverride("font_size", 16);
		infoLabel.AddThemeColorOverride("font_color", new Color(COLOR_TEXT_FAINT, 0.7f));

		AddChild(infoLabel);
	}

	// ============================================
	// Меню настроек
	// ============================================

	private void BuildSettingsUI()
	{
		// Очищаем контейнер настроек
		foreach (var child in _settingsContainer.GetChildren())
		{
			child.QueueFree();
		}

		// Кнопка "Назад"
		var backBtn = new Button();
		backBtn.Text = "< Назад к меню";
		backBtn.Flat = true;
		backBtn.AddThemeFontSizeOverride("font_size", 16);
		backBtn.AddThemeColorOverride("font_color", COLOR_TEXT_DIM);
		backBtn.AddThemeColorOverride("font_hover_color", COLOR_ACCENT);
		backBtn.Alignment = HorizontalAlignment.Left;
		backBtn.CustomMinimumSize = new Vector2(0, 36);
		backBtn.Pressed += () => ShowMainMenu();
		_settingsContainer.AddChild(backBtn);

		var gap = new Control();
		gap.CustomMinimumSize = new Vector2(0, 10);
		_settingsContainer.AddChild(gap);

		var panel = new PanelContainer();
		var style = new StyleBoxFlat();
		style.BgColor = COLOR_PANEL_BG;
		style.BorderColor = COLOR_BORDER_FAINT;
		style.BorderWidthLeft = 2;
		style.BorderWidthRight = 2;
		style.BorderWidthTop = 2;
		style.BorderWidthBottom = 2;
		style.CornerRadiusTopLeft = 4;
		style.CornerRadiusTopRight = 4;
		style.CornerRadiusBottomLeft = 4;
		style.CornerRadiusBottomRight = 4;
		style.ContentMarginLeft = 24;
		style.ContentMarginRight = 24;
		style.ContentMarginTop = 20;
		style.ContentMarginBottom = 20;
		panel.AddThemeStyleboxOverride("panel", style);
		_settingsContainer.AddChild(panel);

		var list = new VBoxContainer();
		list.AddThemeConstantOverride("separation", 16);
		panel.AddChild(list);

		AddSettingsSectionHeader(list, "Аудио");
		AddSettingsSlider(list, "Громкость музыки", "music_volume", 0.7f);
		AddSettingsSlider(list, "Громкость звуков", "sfx_volume", 0.8f);
		AddSettingsSeparator(list);

		AddSettingsSectionHeader(list, "Видео");
		AddSettingsToggle(list, "Полноэкранный режим", "fullscreen", false, OnFullscreenToggled);
		AddSettingsSeparator(list);

		AddSettingsSectionHeader(list, "Язык");
		AddSettingsOption(list, "Язык интерфейса", "language", new string[] { "Русский", "English" }, 0);
	}

	private void AddSettingsSectionHeader(Control parent, string text)
	{
		var header = new Label();
		header.Text = text;
		header.AddThemeFontSizeOverride("font_size", 14);
		header.AddThemeColorOverride("font_color", COLOR_ACCENT);
		parent.AddChild(header);
	}

	private void AddSettingsSeparator(Control parent)
	{
		var sep = new HSeparator();
		sep.AddThemeColorOverride("color", new Color(COLOR_BORDER_FAINT, 0.6f));
		parent.AddChild(sep);
	}

	private void AddSettingsSlider(Control parent, string labelText, string key, float defaultVal)
	{
		var row = new VBoxContainer();
		row.AddThemeConstantOverride("separation", 4);
		parent.AddChild(row);

		var label = new Label();
		label.Text = labelText;
		label.AddThemeFontSizeOverride("font_size", 14);
		label.AddThemeColorOverride("font_color", COLOR_TEXT_DIM);
		row.AddChild(label);

		var slider = new HSlider();
		slider.MinValue = 0.0;
		slider.MaxValue = 1.0;
		slider.Step = 0.01;
		slider.Value = (float)_config.GetValue("settings", key, defaultVal).AsDouble();
		slider.CustomMinimumSize = new Vector2(0, 24);
		row.AddChild(slider);
		
		slider.ValueChanged += (value) =>
		{
			_config.SetValue("settings", key, value);
			ApplyAudioSettings(key, (float)value);
		};
	}

	private void AddSettingsToggle(Control parent, string labelText, string key, bool defaultVal, Action<bool> callback)
	{
		var row = new HBoxContainer();
		parent.AddChild(row);

		var label = new Label();
		label.Text = labelText;
		label.AddThemeFontSizeOverride("font_size", 14);
		label.AddThemeColorOverride("font_color", COLOR_TEXT_DIM);
		label.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
		row.AddChild(label);

		var toggle = new CheckButton();
		toggle.ButtonPressed = (bool)_config.GetValue("settings", key, defaultVal).AsBool();
		row.AddChild(toggle);
		
		toggle.Toggled += (pressed) =>
		{
			_config.SetValue("settings", key, pressed);
			callback?.Invoke(pressed);
		};
	}

	private void AddSettingsOption(Control parent, string labelText, string key, string[] options, int defaultIndex)
	{
		var row = new HBoxContainer();
		parent.AddChild(row);

		var label = new Label();
		label.Text = labelText;
		label.AddThemeFontSizeOverride("font_size", 14);
		label.AddThemeColorOverride("font_color", COLOR_TEXT_DIM);
		label.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
		row.AddChild(label);

		var option = new OptionButton();
		foreach (string opt in options)
		{
			option.AddItem(opt);
		}
		option.Selected = (int)_config.GetValue("settings", key, defaultIndex).AsInt32();
		row.AddChild(option);
		
		option.ItemSelected += (index) =>
		{
			_config.SetValue("settings", key, (int)index);
		};
	}

	// ============================================
	// Модальное окно выхода
	// ============================================

	private void BuildExitModalDialog()
	{
		var modalOverlay = new ColorRect();
		modalOverlay.SetAnchorsPreset(Control.LayoutPreset.FullRect);
		modalOverlay.Color = new Color(0, 0, 0, 0.7f);
		modalOverlay.MouseFilter = Control.MouseFilterEnum.Stop;
		AddChild(modalOverlay);
		modalOverlay.Hide();

		_exitModalPanel = new PanelContainer();
		_exitModalPanel.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.Center);
		_exitModalPanel.CustomMinimumSize = new Vector2(450, 250);
		_exitModalPanel.MouseFilter = Control.MouseFilterEnum.Stop;
		
		var panelStyle = new StyleBoxFlat
		{
			BgColor = COLOR_MODAL_BG,
			BorderColor = COLOR_MODAL_BORDER,
			BorderWidthLeft = 2,
			BorderWidthRight = 2,
			BorderWidthTop = 2,
			BorderWidthBottom = 2,
			CornerRadiusTopLeft = 8,
			CornerRadiusTopRight = 8,
			CornerRadiusBottomLeft = 8,
			CornerRadiusBottomRight = 8,
			ContentMarginLeft = 24,
			ContentMarginRight = 24,
			ContentMarginTop = 20,
			ContentMarginBottom = 20
		};
		_exitModalPanel.AddThemeStyleboxOverride("panel", panelStyle);
		
		AddChild(_exitModalPanel);
		_exitModalPanel.Hide();

		var modalContent = new VBoxContainer();
		modalContent.Alignment = BoxContainer.AlignmentMode.Center;
		modalContent.AddThemeConstantOverride("separation", 20);
		_exitModalPanel.AddChild(modalContent);

		var warningIcon = new Label();
		warningIcon.Text = "⚠";
		warningIcon.AddThemeFontSizeOverride("font_size", 48);
		warningIcon.AddThemeColorOverride("font_color", COLOR_ACCENT_DANGER);
		warningIcon.HorizontalAlignment = HorizontalAlignment.Center;
		modalContent.AddChild(warningIcon);

		var warningLabel = new Label();
		warningLabel.Text = "Вы уверены, что хотите выйти в главное меню?\nНесохранённый прогресс будет потерян!";
		warningLabel.HorizontalAlignment = HorizontalAlignment.Center;
		warningLabel.AutowrapMode = TextServer.AutowrapMode.Word;
		warningLabel.AddThemeFontSizeOverride("font_size", 16);
		warningLabel.AddThemeColorOverride("font_color", COLOR_TEXT_DIM);
		warningLabel.CustomMinimumSize = new Vector2(0, 60);
		modalContent.AddChild(warningLabel);

		var buttonRow = new HBoxContainer();
		buttonRow.Alignment = BoxContainer.AlignmentMode.Center;
		buttonRow.AddThemeConstantOverride("separation", 20);
		modalContent.AddChild(buttonRow);

		_exitModalCancelBtn = CreateModalButton("Отмена", COLOR_TEXT_DIM, COLOR_ACCENT_HOVER);
		_exitModalCancelBtn.Pressed += () => HideExitModalDialog();
		buttonRow.AddChild(_exitModalCancelBtn);

		_exitModalConfirmBtn = CreateModalButton("Выйти", COLOR_ACCENT_DANGER, COLOR_ACCENT_DANGER_HOVER);
		_exitModalConfirmBtn.Pressed += () => ConfirmExitToMainMenu();
		buttonRow.AddChild(_exitModalConfirmBtn);

		_exitModalPanel.SetMeta("overlay", modalOverlay);
		
		CallDeferred(nameof(CenterExitModalPanel));
	}

	private void CenterExitModalPanel()
	{
		_exitModalPanel.OffsetLeft = -_exitModalPanel.Size.X / 2;
		_exitModalPanel.OffsetTop = -_exitModalPanel.Size.Y / 2;
		_exitModalPanel.OffsetRight = _exitModalPanel.Size.X / 2;
		_exitModalPanel.OffsetBottom = _exitModalPanel.Size.Y / 2;
	}

	private Button CreateModalButton(string text, Color textColor, Color hoverColor)
	{
		var btn = new Button();
		btn.Text = text;
		btn.CustomMinimumSize = new Vector2(180, 45);
		btn.AddThemeFontSizeOverride("font_size", 16);
		btn.AddThemeColorOverride("font_color", textColor);
		btn.AddThemeColorOverride("font_hover_color", hoverColor);
		btn.Alignment = HorizontalAlignment.Center;
		btn.Flat = false;

		var styleNormal = new StyleBoxFlat
		{
			BgColor = COLOR_BUTTON_BG,
			BorderColor = COLOR_BUTTON_BORDER,
			BorderWidthLeft = 1,
			BorderWidthRight = 1,
			BorderWidthTop = 1,
			BorderWidthBottom = 1,
			CornerRadiusTopLeft = 4,
			CornerRadiusTopRight = 4,
			CornerRadiusBottomLeft = 4,
			CornerRadiusBottomRight = 4,
			ContentMarginLeft = 16,
			ContentMarginRight = 16
		};
		btn.AddThemeStyleboxOverride("normal", styleNormal);

		var styleHover = (StyleBoxFlat)styleNormal.Duplicate();
		styleHover.BgColor = new Color(0.12f, 0.10f, 0.14f, 0.8f);
		styleHover.BorderColor = hoverColor;
		btn.AddThemeStyleboxOverride("hover", styleHover);

		var stylePressed = (StyleBoxFlat)styleNormal.Duplicate();
		stylePressed.BgColor = new Color(0.06f, 0.05f, 0.08f, 0.9f);
		stylePressed.BorderColor = textColor;
		btn.AddThemeStyleboxOverride("pressed", stylePressed);

		btn.FocusMode = Control.FocusModeEnum.All;
		
		return btn;
	}

	private void ShowExitModalDialog()
	{
		var overlay = (ColorRect)_exitModalPanel.GetMeta("overlay");
		overlay.Show();
		_exitModalPanel.Show();
		_exitModalCancelBtn.GrabFocus();
	}

	private void HideExitModalDialog()
	{
		var overlay = (ColorRect)_exitModalPanel.GetMeta("overlay");
		overlay.Hide();
		_exitModalPanel.Hide();
		
		if (_buttons.Count >= 4)
			_buttons[3].GrabFocus();
	}

	private void ConfirmExitToMainMenu()
	{
		SaveConfig();
		GetTree().Paused = false;
		HideExitModalDialog();
		sceneManager?.ChangeScene(MAIN_MENU_SCENE_PATH);
	}

	// ============================================
	// Навигация между меню
	// ============================================

	private void ShowMainMenu()
	{
		_titleLabel.Text = "ПАУЗА";
		_mainMenuContainer.Visible = true;
		_settingsContainer.Visible = false;
		
		if (_buttons.Count > 0)
			_buttons[0].GrabFocus();
	}

	private void ShowSettingsMenu()
	{
		_titleLabel.Text = "НАСТРОЙКИ";
		_mainMenuContainer.Visible = false;
		_settingsContainer.Visible = true;
		
		BuildSettingsUI();
	}

	// ============================================
	// Кнопки главного меню
	// ============================================

	private void AddMenuButton(Control parent, string text, Action callback, bool isPrimary)
	{
		var btn = new Button();
		btn.Text = text;
		btn.CustomMinimumSize = new Vector2(0, 50);
		btn.SizeFlagsHorizontal = Control.SizeFlags.Fill;
		btn.SizeFlagsVertical = Control.SizeFlags.ShrinkCenter;

		btn.AddThemeFontSizeOverride("font_size", 18);
		btn.AddThemeColorOverride("font_color", isPrimary ? COLOR_ACCENT : COLOR_TEXT_DIM);
		btn.AddThemeColorOverride("font_hover_color", COLOR_ACCENT_HOVER);
		btn.AddThemeColorOverride("font_pressed_color", COLOR_ACCENT);
		btn.AddThemeColorOverride("font_focus_color", COLOR_ACCENT_HOVER);

		btn.Alignment = HorizontalAlignment.Center;
		btn.Flat = false;

		var styleNormal = new StyleBoxFlat
		{
			BgColor = COLOR_BUTTON_BG,
			BorderWidthLeft = 1,
			BorderWidthRight = 1,
			BorderWidthTop = 1,
			BorderWidthBottom = 1,
			BorderColor = COLOR_BUTTON_BORDER,
			CornerRadiusTopLeft = 4,
			CornerRadiusTopRight = 4,
			CornerRadiusBottomLeft = 4,
			CornerRadiusBottomRight = 4,
			ContentMarginLeft = 24,
			ContentMarginRight = 24
		};

		btn.AddThemeStyleboxOverride("normal", styleNormal);

		var styleHover = (StyleBoxFlat)styleNormal.Duplicate();
		styleHover.BgColor = new Color(0.12f, 0.10f, 0.14f, 0.8f);
		styleHover.BorderColor = COLOR_ACCENT;
		btn.AddThemeStyleboxOverride("hover", styleHover);

		var stylePressed = (StyleBoxFlat)styleNormal.Duplicate();
		stylePressed.BgColor = new Color(0.06f, 0.05f, 0.08f, 0.9f);
		stylePressed.BorderColor = COLOR_ACCENT.Darkened(0.3f);
		btn.AddThemeStyleboxOverride("pressed", stylePressed);

		var styleFocus = (StyleBoxFlat)styleNormal.Duplicate();
		styleFocus.BorderColor = new Color(COLOR_ACCENT, 0.6f);
		btn.AddThemeStyleboxOverride("focus", styleFocus);

		btn.FocusMode = Control.FocusModeEnum.All;

		parent.AddChild(btn);
		btn.Pressed += callback;

		_buttons.Add(btn);
	}

	private void SetupShortcuts()
	{
		if (_buttons.Count > 0)
			_buttons[0].GrabFocus();
	}

	// Обработчики
	private void OnContinuePressed()
	{
		SaveConfig();
		HideMenu();
	}

	private void OnSavePressed()
	{
		GD.Print("Сохранение игры...");
		// Здесь будет логика сохранения
	}

	private void OnExitToMainMenuPressed()
	{
		ShowExitModalDialog();
	}

	private void OnFullscreenToggled(bool pressed)
	{
		if (pressed)
		{
			DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
		}
		else
		{
			DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
		}
	}

	private void ApplyAudioSettings(string key, float value)
	{
		if (key == "music_volume")
		{
			int busIndex = AudioServer.GetBusIndex("Music");
			if (busIndex >= 0)
				AudioServer.SetBusVolumeDb(busIndex, Mathf.LinearToDb(value));
		}
		else if (key == "sfx_volume")
		{
			int busIndex = AudioServer.GetBusIndex("SFX");
			if (busIndex >= 0)
				AudioServer.SetBusVolumeDb(busIndex, Mathf.LinearToDb(value));
		}
	}

	private void SaveConfig()
	{
		var error = _config.Save(CONFIG_PATH);
		if (error != Error.Ok)
		{
			GD.PrintErr($"Failed to save config: {error}");
		}
	}
}
