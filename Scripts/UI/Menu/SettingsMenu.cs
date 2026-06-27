using Godot;
using System;
using System.Collections.Generic;

public partial class SettingsMenu : Control
{
	private const string PATH_MAIN_MENU = "res://Scenes/UI/Menu/MainMenu.tscn";
	private const string CONFIG_PATH = "user://settings.cfg";

	private static readonly Color ACCENT = new Color(0.82f, 0.30f, 0.18f);
	private static readonly Color TEXT_DIM = new Color(0.62f, 0.56f, 0.52f);
	private static readonly Color BORDER_FAINT = new Color(0.22f, 0.18f, 0.18f);
	private static readonly Color COLOR_BG_OVERLAY = new Color(0.02f, 0.02f, 0.04f, 0.55f);

	private MusicManager _musicManager;
	private ConfigFile _config = new ConfigFile();
	private SceneManager _sceneManager;

	public override void _Ready()
	{
		_musicManager = GetNode<MusicManager>("/root/MusicManager");

		_sceneManager = GetNode<SceneManager>("/root/SceneManager");
		
		SetAnchorsPreset(LayoutPreset.FullRect);
		
		// Загружаем конфигурацию
		var error = _config.Load(CONFIG_PATH);
		if (error != Error.Ok)
		{
			GD.Print($"Config file not found, using defaults. Error: {error}");
		}
		
		BuildUI();
	}

	private void BuildUI()
	{
		// === Фоновый слой ===
		var bgImage = new TextureRect();
		bgImage.SetAnchorsPreset(LayoutPreset.FullRect);
		bgImage.Texture = GD.Load<Texture2D>("res://Assets/Backgrounds/BackgroundMain.png");
		bgImage.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
		bgImage.StretchMode = TextureRect.StretchModeEnum.Scale;
		AddChild(bgImage);

		var overlay = new ColorRect();
		overlay.SetAnchorsPreset(LayoutPreset.FullRect);
		overlay.Color = COLOR_BG_OVERLAY;
		AddChild(overlay);

		// === Декоративная рамка ===
		var border = new PanelContainer();
		border.SetAnchorsPreset(LayoutPreset.FullRect);
		
		var borderStyle = new StyleBoxFlat();
		borderStyle.BgColor = Colors.Transparent;
		borderStyle.BorderWidthLeft = 2;
		borderStyle.BorderWidthRight = 2;
		borderStyle.BorderColor = new Color(0.3f, 0.25f, 0.3f, 0.2f);
		borderStyle.ContentMarginLeft = 2;
		borderStyle.ContentMarginRight = 2;
		border.AddThemeStyleboxOverride("panel", borderStyle);
		AddChild(border);

		var root = new VBoxContainer();
		root.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
		root.AddThemeConstantOverride("separation", 24);
		AddChild(root);

		// === Верхняя панель ===
		var topBar = new HBoxContainer();
		topBar.CustomMinimumSize = new Vector2(0, 64);
		root.AddChild(topBar);

		var backBtn = new Button();
		backBtn.Text = "< Назад";
		backBtn.Flat = true;
		backBtn.AddThemeFontSizeOverride("font_size", 18);
		backBtn.AddThemeColorOverride("font_color", TEXT_DIM);
		backBtn.AddThemeColorOverride("font_hover_color", ACCENT);
		backBtn.CustomMinimumSize = new Vector2(140, 0);
		backBtn.Pressed += () =>
		{
			SaveConfig();
			_sceneManager?.ChangeScene(PATH_MAIN_MENU);
		};
		topBar.AddChild(backBtn);

		var title = new Label();
		title.Text = "НАСТРОЙКИ";
		title.AddThemeFontSizeOverride("font_size", 26);
		title.AddThemeColorOverride("font_color", TEXT_DIM);
		title.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
		title.HorizontalAlignment = HorizontalAlignment.Center;
		topBar.AddChild(title);

		var spacerR = new Control();
		spacerR.CustomMinimumSize = new Vector2(140, 0);
		topBar.AddChild(spacerR);

		// === Центральная область ===
		var center = new CenterContainer();
		center.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
		root.AddChild(center);

		var panel = new PanelContainer();
		panel.CustomMinimumSize = new Vector2(480, 0);
		
		var style = new StyleBoxFlat();
		style.BgColor = new Color(0.05f, 0.035f, 0.04f, 0.92f);
		style.BorderColor = BORDER_FAINT;
		style.BorderWidthLeft = 2;
		style.BorderWidthRight = 2;
		style.BorderWidthTop = 2;
		style.BorderWidthBottom = 2;
		style.CornerRadiusTopLeft = 2;
		style.CornerRadiusTopRight = 2;
		style.CornerRadiusBottomLeft = 2;
		style.CornerRadiusBottomRight = 2;
		style.ContentMarginLeft = 28;
		style.ContentMarginRight = 28;
		style.ContentMarginTop = 24;
		style.ContentMarginBottom = 24;
		panel.AddThemeStyleboxOverride("panel", style);
		center.AddChild(panel);

		var list = new VBoxContainer();
		list.AddThemeConstantOverride("separation", 22);
		panel.AddChild(list);

		// Добавляем настройки
		AddSliderRow(list, "Громкость музыки", "music_volume", 0.7f);
		AddSliderRow(list, "Громкость звуков", "sfx_volume", 0.8f);
		AddSeparator(list);
		AddToggleRow(list, "Полноэкранный режим", "fullscreen", false, OnFullscreenToggled);
		AddSeparator(list);
		AddOptionRow(list, "Язык", "language", new string[] { "Русский", "English" }, 0);
	}

	private void AddSeparator(Control parent)
	{
		var sep = new HSeparator();
		sep.AddThemeColorOverride("color", new Color(BORDER_FAINT, 0.6f));
		parent.AddChild(sep);
	}

	private void AddSliderRow(Control parent, string labelText, string key, float defaultVal)
	{
		var row = new VBoxContainer();
		row.AddThemeConstantOverride("separation", 6);
		parent.AddChild(row);

		var label = new Label();
		label.Text = labelText;
		label.AddThemeColorOverride("font_color", TEXT_DIM);
		row.AddChild(label);

		var slider = new HSlider();
		slider.MinValue = 0.0;
		slider.MaxValue = 1.0;
		slider.Step = 0.01;
		slider.Value = (float)_config.GetValue("settings", key, defaultVal).AsDouble();
		slider.CustomMinimumSize = new Vector2(0, 20);
		if (_musicManager != null)
		{
			_musicManager.SetVolume((float)slider.Value);
		}
		row.AddChild(slider);
		
		slider.ValueChanged += (value) =>
		{
			if (key == "music_volume" && _musicManager != null)
			{
				GD.Print((float)value);
				_musicManager.SetVolume((float)value);
			}
		};
	}

	private void AddToggleRow(Control parent, string labelText, string key, bool defaultVal, Action<bool> callback)
	{
		var row = new HBoxContainer();
		parent.AddChild(row);

		var label = new Label();
		label.Text = labelText;
		label.AddThemeColorOverride("font_color", TEXT_DIM);
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

	private void AddOptionRow(Control parent, string labelText, string key, string[] options, int defaultIndex)
	{
		var row = new HBoxContainer();
		parent.AddChild(row);

		var label = new Label();
		label.Text = labelText;
		label.AddThemeColorOverride("font_color", TEXT_DIM);
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

	private void SaveConfig()
	{
		var error = _config.Save(CONFIG_PATH);
		if (error != Error.Ok)
		{
			GD.PrintErr($"Failed to save config: {error}");
		}
	}
}
