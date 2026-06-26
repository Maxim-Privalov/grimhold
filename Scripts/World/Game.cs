using Godot;
using System;

public partial class Game : Node2D
{
	private const string _currentLevelPth = "res://Scenes/World/Levels/LevelOne.tscn";
	public override void _Ready()
	{	

		PackedScene levelScene = GD.Load<PackedScene>(_currentLevelPth);
		var _currentLevel = levelScene.Instantiate<Node2D>();
		AddChild(_currentLevel);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
