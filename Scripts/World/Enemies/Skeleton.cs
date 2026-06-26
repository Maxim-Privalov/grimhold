using Godot;

public partial class Skeleton : CharacterBody2D
{
    [Export] public float WalkSpeed = 1.0f;//временная скорость, поменять после теста
    [Export] public float ChaseSpeed = 2.0f;//временная скорость, поменять после теста
    [Export] public float PatrolChangeTime = 2.0f;//временно, поменять после теста

    private Area2D _visionArea;
    private Sprite2D _sprite;
    private Player _targetPlayer = null;

    private float _patrolTimer = 0.0f;
    private int _moveDirection = 1; 
    private float _spawnY = 0.0f; 

    public override void _Ready()
    {
        _spawnY = GlobalPosition.Y;
        _visionArea = GetNode<Area2D>("VisionArea");
        _sprite = GetNode<Sprite2D>("Sprite2D");
        _visionArea.BodyEntered += OnBodyEntered;
        _visionArea.BodyExited += OnBodyExited;
    }

    public override void _PhysicsProcess(double delta)
    {
        Vector2 velocity = Velocity;
        if (!IsOnFloor())
        {
            velocity += GetGravity() * (float)delta;
        }
        if (_targetPlayer != null && IsPlayerOnSamePlatform())
        {
            float directionToPlayer = Mathf.Sign(_targetPlayer.GlobalPosition.X - GlobalPosition.X);
            velocity.X = directionToPlayer * ChaseSpeed;
        }
        else
        {
            _patrolTimer -= (float)delta;
            if (_patrolTimer <= 0.0f)
            {
                ChooseRandomNewState();
            }
            velocity.X = _moveDirection * WalkSpeed;
        }

        Velocity = velocity;
        MoveAndSlide();
        if (Velocity.X > 0)
        {
            _sprite.FlipH = false; 
        }
        else if (Velocity.X < 0)
        {
            _sprite.FlipH = true;
        }
    }

    private bool IsPlayerOnSamePlatform()
    {
        return Mathf.Abs(_targetPlayer.GlobalPosition.Y - GlobalPosition.Y) < 15.0f;
    }

    private void ChooseRandomNewState()
    {
        _patrolTimer = (float)GD.RandRange(1.0f, PatrolChangeTime);
        int state = (int)(GD.Randi() % 3);
        switch (state)
        {
            case 0: _moveDirection = 0; break;
            case 1: _moveDirection = 1; break;
            case 2: _moveDirection = -1; break;
        }
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body is Player player)
        {
            _targetPlayer = player;
        }
    }

    private void OnBodyExited(Node2D body)
    {
        if (body == _targetPlayer)
        {
            LoseInterest();
        }
    }

    private void LoseInterest()
    {
        _targetPlayer = null;
        ChooseRandomNewState();
    }
}