using Godot;
using System;
using System.Collections.Generic;

public partial class MainMenu : Control
{
	// Пути к сценам
	private const string PATH_SAVE_SLOTS = "res://Scenes/UI/Menu/SaveSlotsMenu.tscn";
	private const string PATH_SETTINGS = "res://Scenes/UI/Menu/SettingsMenu.tscn";
    private const string MainMenuMusicPath = "res://Assets/Audio/Ambient/Menu.mp3";

	// Цветовые константы
	private static readonly Color COLOR_ACCENT = new Color(0.82f, 0.30f, 0.18f);
	private static readonly Color COLOR_ACCENT_HOVER = new Color(0.95f, 0.45f, 0.25f);
	private static readonly Color COLOR_ACCENT_CORRUPT = new Color(0.55f, 0.20f, 0.65f);
	private static readonly Color COLOR_TEXT = new Color(0.85f, 0.82f, 0.78f);
	private static readonly Color COLOR_TEXT_DIM = new Color(0.62f, 0.56f, 0.52f);
	private static readonly Color COLOR_TEXT_FAINT = new Color(0.40f, 0.34f, 0.34f);
	private static readonly Color COLOR_BG_OVERLAY = new Color(0.02f, 0.02f, 0.04f, 0.55f);
	private static readonly Color COLOR_BUTTON_BG = new Color(0.08f, 0.07f, 0.09f, 0.65f);
	private static readonly Color COLOR_BUTTON_BORDER = new Color(0.25f, 0.22f, 0.25f, 0.4f);

	private List<Button> _buttons = new List<Button>();

    private MusicManager _musicManager;

	public override void _Ready()
	{
        _musicManager = GetNode<MusicManager>("/root/MusicManager");

        if (_musicManager != null)
        {
            _musicManager.ChangeMusicByPath(MainMenuMusicPath);
        }

		SetAnchorsPreset(LayoutPreset.FullRect);
		BuildUI();
		SetupShortcuts();
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

		// === Основной контейнер ===
		var root = new VBoxContainer();
		root.Alignment = BoxContainer.AlignmentMode.Center;
		root.AddThemeConstantOverride("separation", 22);
		root.SetAnchorsPreset(LayoutPreset.FullRect);
		border.AddChild(root);

		// Растягивающийся спейсер сверху
		var spacerTop = new Control();
		spacerTop.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
		root.AddChild(spacerTop);

		// === Логотип ===
		var titleWrap = new VBoxContainer();
		titleWrap.Alignment = BoxContainer.AlignmentMode.Center;
		titleWrap.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;
		root.AddChild(titleWrap);

		var title = new TextureRect();
		title.Texture = GD.Load<Texture2D>("res://Assets/Others/titleLogo.png");
		title.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
		title.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
		title.CustomMinimumSize = new Vector2(
			title.Texture.GetWidth() / 2.5f, 
			title.Texture.GetHeight() / 2.5f
		);
		titleWrap.AddChild(title);

		// Отступ после логотипа
		var gapAfterLogo = new Control();
		gapAfterLogo.CustomMinimumSize = new Vector2(0, 20);
		root.AddChild(gapAfterLogo);

		// === Меню с кнопками ===
		var menuBox = new VBoxContainer();
		menuBox.Alignment = BoxContainer.AlignmentMode.Center;
		menuBox.AddThemeConstantOverride("separation", 10);
		menuBox.CustomMinimumSize = new Vector2(340, 0);
		menuBox.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;
		root.AddChild(menuBox);

		AddMenuButton(menuBox, "Новая игра", () => OnNewGamePressed(), false);
		AddMenuButton(menuBox, "Продолжить", () => OnContinuePressed(), false);
		AddMenuButton(menuBox, "Настройки", () => OnSettingsPressed(), false);
		AddMenuButton(menuBox, "Выйти", () => OnQuitPressed(), false);

		// Растягивающийся спейсер снизу
		var spacerBottom = new Control();
		spacerBottom.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
		root.AddChild(spacerBottom);

		// === Версия ===
		var version = new Label();
		version.Text = "build 0.1.0";
		version.AddThemeFontSizeOverride("font_size", 20);
		version.AddThemeColorOverride("font_color", new Color(COLOR_TEXT_FAINT, 0.7f));
		version.HorizontalAlignment = HorizontalAlignment.Right;
		version.VerticalAlignment = VerticalAlignment.Bottom;

		// Привязываем к правому нижнему углу
		version.AnchorLeft = 1.0f;
		version.AnchorRight = 1.0f;
		version.AnchorTop = 1.0f;
		version.AnchorBottom = 1.0f;
		version.OffsetLeft = -160;   // ширина области
		version.OffsetRight = -16;   // отступ справа
		version.OffsetTop = -32;     // высота + отступ снизу
		version.OffsetBottom = -8;   // отступ снизу

		AddChild(version);  // Добавляем напрямую, а не в root

		// Дополнительный отступ снизу
		var bottomPadding = new Control();
		bottomPadding.CustomMinimumSize = new Vector2(0, 24);
		root.AddChild(bottomPadding);
	}

	private void AddMenuButton(Control parent, string text, Action callback, bool isPrimary)
	{
		var btn = new Button();
		btn.Text = text;
		btn.CustomMinimumSize = new Vector2(0, 50);
		btn.SizeFlagsHorizontal = Control.SizeFlags.Fill;
		btn.SizeFlagsVertical = Control.SizeFlags.ShrinkCenter;

		// Шрифт
		btn.AddThemeFontSizeOverride("font_size", 18);
		btn.AddThemeColorOverride("font_color", isPrimary ? COLOR_TEXT : COLOR_TEXT_DIM);
		btn.AddThemeColorOverride("font_hover_color", COLOR_ACCENT_HOVER);
		btn.AddThemeColorOverride("font_pressed_color", COLOR_ACCENT);
		btn.AddThemeColorOverride("font_focus_color", COLOR_ACCENT_HOVER);

		// Выравнивание текста — по центру
		btn.Alignment = HorizontalAlignment.Center;

		// Плоский режим и стили
		btn.Flat = false;

		// Нормальное состояние
		var styleNormal = new StyleBoxFlat();
		styleNormal.BgColor = COLOR_BUTTON_BG;
		styleNormal.BorderWidthLeft = 1;
		styleNormal.BorderWidthRight = 1;
		styleNormal.BorderWidthTop = 1;
		styleNormal.BorderWidthBottom = 1;
		styleNormal.BorderColor = COLOR_BUTTON_BORDER;
		styleNormal.CornerRadiusTopLeft = 4;
		styleNormal.CornerRadiusTopRight = 4;
		styleNormal.CornerRadiusBottomLeft = 4;
		styleNormal.CornerRadiusBottomRight = 4;
		styleNormal.ContentMarginLeft = 24;
		styleNormal.ContentMarginRight = 24;
		btn.AddThemeStyleboxOverride("normal", styleNormal);

		// Состояние наведения
		var styleHover = (StyleBoxFlat)styleNormal.Duplicate();
		styleHover.BgColor = new Color(0.12f, 0.10f, 0.14f, 0.8f);
		styleHover.BorderColor = COLOR_ACCENT;
		btn.AddThemeStyleboxOverride("hover", styleHover);

		// Состояние нажатия
		var stylePressed = (StyleBoxFlat)styleNormal.Duplicate();
		stylePressed.BgColor = new Color(0.06f, 0.05f, 0.08f, 0.9f);
		stylePressed.BorderColor = COLOR_ACCENT.Darkened(0.3f);
		btn.AddThemeStyleboxOverride("pressed", stylePressed);

		// Состояние фокуса
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
		// Горячие клавиши
		if (!InputMap.HasAction("ui_accept"))
			return;

		// Первая кнопка получает фокус автоматически
		if (_buttons.Count > 0)
			_buttons[0].GrabFocus();
	}

	private void OnNewGamePressed()
	{
		SaveSlotsMenuModeNew();
		SceneManager.Instance?.ChangeScene(PATH_SAVE_SLOTS);
	}

	private void OnContinuePressed()
	{
		SaveSlotsMenuModeContinue();
		SceneManager.Instance?.ChangeScene(PATH_SAVE_SLOTS);
	}

	private void OnSettingsPressed()
	{
		SceneManager.Instance?.ChangeScene(PATH_SETTINGS);
	}

	private void OnQuitPressed()
	{
		SceneManager.Instance?.QuitGame();
	}

	private void SaveSlotsMenuModeNew()
	{
		GetTree().SetMeta("save_slots_mode", "new");
	}

	private void SaveSlotsMenuModeContinue()
	{
		GetTree().SetMeta("save_slots_mode", "continue");
	}
}
