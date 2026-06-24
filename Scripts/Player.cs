using Godot;

public partial class Player : CharacterBody2D
{
	[Export]
	public float Speed = 250f;

	[Export]
	public float JumpVelocity = -450f;

	[Export]
	public float Gravity = 1200f;

	// Номер слоя, на котором будут платформы, через которые можно спускаться.
	// В Godot слои считаются с 1, поэтому 2 = второй слой.
	[Export]
	public int DropThroughPlatformLayer = 2;

	[Export]
	public float DropThroughTime = 0.25f;

	[Export]
	public float DropVelocity = 80f;

	private bool _isDroppingThroughPlatform = false;

	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;

		if (!IsOnFloor())
		{
			velocity.Y += Gravity * (float)delta;
		}

		float direction = 0;

		if (Input.IsActionPressed("moveLeft"))
		{
			// меняем направление спрайта на движение влево
			direction -= 1;
		}

		if (Input.IsActionPressed("moveRight"))
		{
			// меняем направление спрайта на движение вправо
			direction += 1;
		}

		velocity.X = direction * Speed;

		if (Input.IsActionJustPressed("moveUp") && IsOnFloor())
		{
			if (Input.IsActionPressed("moveDown"))
			{
				
				StartDropThroughPlatform();

				
				velocity.Y = DropVelocity;
			}
			else
			{
				
				velocity.Y = JumpVelocity;
			}
		}

		Velocity = velocity;
		MoveAndSlide();
	}

	private async void StartDropThroughPlatform()
	{
		if (_isDroppingThroughPlatform)
			return;

		_isDroppingThroughPlatform = true;

		// Временно отключаем столкновение игрока со слоем платформ.
		SetCollisionMaskValue(DropThroughPlatformLayer, false);

		await ToSignal(
			GetTree().CreateTimer(DropThroughTime),
			SceneTreeTimer.SignalName.Timeout
		);

		// Возвращаем столкновение обратно.
		SetCollisionMaskValue(DropThroughPlatformLayer, true);

		_isDroppingThroughPlatform = false;
	}
}
