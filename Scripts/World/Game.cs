using Godot;
using System;

public partial class Game : Node2D
{
	private const string escMenuPth = "res://Scenes/UI/GameMenu/EscMenu.tscn";
	private const string _currentLevelPth = "res://Scenes/World/Levels/LevelOne.tscn";

	CanvasLayer escMenu = null;

	public override void _Ready()
	{	
		PackedScene levelScene = GD.Load<PackedScene>(_currentLevelPth);
		var _currentLevel = levelScene.Instantiate<Node2D>();
		AddChild(_currentLevel);

		PackedScene escMenuScene = GD.Load<PackedScene>(escMenuPth);
		escMenu = escMenuScene.Instantiate<CanvasLayer>();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("enterEscMenu") && escMenu != null)
		{
			AddChild(escMenu);
		}
	}
}
