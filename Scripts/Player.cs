using Godot;

public partial class Player : CharacterBody2D
{
	[Signal]
	public delegate void HealthChangedEventHandler(int currentHealth, int maxHealth);

	[Export]
	public float Speed = 650f;

	[Export]
	public float JumpVelocity = -850f;

	[Export]
	public float Gravity = 1200f;

	[Export]
	public float Acceleration = 1800f;

	[Export]
	public float Friction = 2200f;

	[Export]
	public int DropThroughPlatformLayer = 2;

	[Export]
	public float DropThroughTime = 0.25f;

	[Export]
	public float DropVelocity = 80f;

	[Export]
	public int MaxHealth = 3;

	[Export]
	public float DamageCooldown = 0.7f;

	public int CurrentHealth;

	private bool _isDroppingThroughPlatform = false;
	private bool _canTakeDamage = true;
	private bool _isDead = false;

	private AnimatedSprite2D _animatedSprite;

	public override void _Ready()
	{
		_animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");

		CurrentHealth = MaxHealth;

		EmitSignal(SignalName.HealthChanged, CurrentHealth, MaxHealth);
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_isDead)
			return;

		Vector2 velocity = Velocity;

		if (!IsOnFloor())
		{
			velocity.Y += Gravity * (float)delta;
		}

		float direction = 0;

		if (Input.IsActionPressed("moveLeft"))
		{
			direction -= 1;
			_animatedSprite.FlipH = true;
		}

		if (Input.IsActionPressed("moveRight"))
		{
			direction += 1;
			_animatedSprite.FlipH = false;
		}

		if (direction != 0)
		{
			velocity.X = Mathf.MoveToward(
				velocity.X,
				direction * Speed,
				Acceleration * (float)delta
			);
		}
		else
		{
			velocity.X = Mathf.MoveToward(
				velocity.X,
				0,
				Friction * (float)delta
			);
		}

		if (Input.IsActionPressed("moveDown"))
		{
			StartDropThroughPlatform();
		}

		if (Input.IsActionJustPressed("moveUp") && IsOnFloor())
		{
			velocity.Y = JumpVelocity;
		}

		Velocity = velocity;
		MoveAndSlide();

		UpdateAnimation(direction);
	}

	public async void TakeDamage(int damage = 1)
	{
		if (!_canTakeDamage || _isDead)
			return;

		_canTakeDamage = false;

		CurrentHealth -= damage;

		if (CurrentHealth < 0)
			CurrentHealth = 0;

		GD.Print("Player HP: " + CurrentHealth);

		EmitSignal(SignalName.HealthChanged, CurrentHealth, MaxHealth);

		if (CurrentHealth <= 0)
		{
			Die();
			return;
		}

		await ToSignal(
			GetTree().CreateTimer(DamageCooldown),
			SceneTreeTimer.SignalName.Timeout
		);

		_canTakeDamage = true;
	}

	public void Heal(int amount = 1)
	{
		if (_isDead)
			return;

		CurrentHealth += amount;

		if (CurrentHealth > MaxHealth)
			CurrentHealth = MaxHealth;

		EmitSignal(SignalName.HealthChanged, CurrentHealth, MaxHealth);
	}

	private void Die()
	{
		if (_isDead)
			return;

		_isDead = true;

		GD.Print("Player died");

		// Пока что просто перезапускаем сцену
		GetTree().ReloadCurrentScene();
	}

	private void UpdateAnimation(float direction)
	{
		if (!IsOnFloor())
		{
			if (Velocity.Y < 0)
			{
				PlayAnimation("jump");
			}
			else
			{
				PlayAnimation("drop");
			}
		}
		else if (direction != 0)
		{
			PlayAnimation("run");
		}
		else
		{
			PlayAnimation("default");
		}
	}

	private void PlayAnimation(string animationName)
	{
		if (_animatedSprite.Animation != animationName)
		{
			_animatedSprite.Play(animationName);
		}
	}

	private async void StartDropThroughPlatform()
	{
		if (_isDroppingThroughPlatform)
			return;

		_isDroppingThroughPlatform = true;

		SetCollisionMaskValue(DropThroughPlatformLayer, false);

		await ToSignal(
			GetTree().CreateTimer(DropThroughTime),
			SceneTreeTimer.SignalName.Timeout
		);

		SetCollisionMaskValue(DropThroughPlatformLayer, true);

		_isDroppingThroughPlatform = false;
	}
}
