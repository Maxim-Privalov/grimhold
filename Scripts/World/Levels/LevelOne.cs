using Godot;
using System;

public partial class LevelOne : Node2D
{
	private const string LEVEL_ONE_BG = "res://Assets/Backgrounds/background1.png";
	private const string LEVEL_ONE_MUSIC = "res://Assets/Audio/Ambient/Lvl1.mp3";
	private MusicManager _musicManager;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{	
		_musicManager = GetNode<MusicManager>("/root/MusicManager");

		_musicManager.ChangeMusicByPath(LEVEL_ONE_MUSIC);

		CanvasLayer canvasLayer = new();
		canvasLayer.Layer = -1;
		AddChild(canvasLayer);
		
		var bg = new TextureRect
		{
			Texture = GD.Load<Texture2D>(LEVEL_ONE_BG),
			StretchMode = TextureRect.StretchModeEnum.Scale,
		};
		bg.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
		
		canvasLayer.AddChild(bg);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
