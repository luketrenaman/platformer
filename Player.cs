using Godot;
using System;
using static Godot.TextServer;

public partial class Player : CharacterBody2D
{
	public float Speed = 300.0f;
	public const float JumpVelocity = -450.0f;
	private double coyoteTime = 0;
	private const double MAX_COYOTE_TIME = 10 / 60f; // 10 physics frames
	private int jumpsUsed = 0;
	private const int MAX_JUMPS = 2;
	private Vector2 DoubleJumpDirection;
	// Mutually exclusive states which determine animations
	private enum PLAYER_STATE { ON_WALL, ON_FLOOR, JUMPING, ROLLING, IDLE, FALLING };
	private PLAYER_STATE currentState;
	private PLAYER_STATE previousState;


	private AnimatedSprite2D animationNode;

	// Get the gravity from the project settings to be synced with RigidBody nodes.
	public float gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();
	public void OnAbilityPickup (Vector2 direction)
	{
        DoubleJumpDirection = direction;
		var ability = GetParent().GetNode<Sprite2D>("CurrentPower");
		ability.Rotation = Mathf.Atan2(direction.Y, direction.X);
		ability.Visible = true;
    }
    private Vector2 UseMidairAbility()
	{
        currentState = PLAYER_STATE.ROLLING;
        return DoubleJumpDirection * Speed * 2.5f;
    }

    private float SpeedIfOnFloor()
	{
		return IsOnFloor() ? Speed : Speed / 2.0f;
	}
	private void SetAnimation(string name)
	{
		animationNode.Play(name);
	}
	private void SetAnimationBasedOnState()
	{
        if (previousState == currentState)
        {
            return;
        }
        switch (currentState)
        {
            case PLAYER_STATE.JUMPING:
                SetAnimation("jump");
                break;
            case PLAYER_STATE.ON_FLOOR:
                    SetAnimation("run");
                break;
			case PLAYER_STATE.IDLE:
				SetAnimation("idle");
				break;
            case PLAYER_STATE.ON_WALL:
                SetAnimation("wall_jump");
                break;
            case PLAYER_STATE.ROLLING:
                SetAnimation("roll");
                break;
            default:
                SetAnimation("fall");
                break;
        }
    }
	public override void _Ready()
	{
		animationNode = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		DoubleJumpDirection = new Vector2(0, 0);
    }
	public bool DirectionOppositeOfIntent(Vector2 direction)
    {
		return direction.X != 0 && Math.Sign(direction.X) != Math.Sign(DoubleJumpDirection.X);
	}
    public override void _PhysicsProcess(double delta)
    {
		Vector2 velocity = Velocity;
        // Handle Jump.
        if (Input.IsActionJustPressed("ui_accept") && jumpsUsed < MAX_JUMPS)
        {
			if(coyoteTime >= 0 && jumpsUsed == 0)
			{
                // Standard jump
                velocity.Y = JumpVelocity;
				currentState = PLAYER_STATE.JUMPING;
			}
			// If double jump is not null
			else if(!DoubleJumpDirection.Equals(Vector2.Zero))
			{
				// Dive Jump
				velocity = UseMidairAbility();
			}
            jumpsUsed++;
        }
        // Add the gravity.
        if (IsOnFloor())
		{
			coyoteTime = MAX_COYOTE_TIME;
			jumpsUsed = 0;
			currentState = PLAYER_STATE.ON_FLOOR;
        }
		else if (IsOnWall())
		{
            coyoteTime = MAX_COYOTE_TIME;
            jumpsUsed = 0;
			if (velocity.Y > 0)
			{
				velocity.Y += gravity * (float)delta * 0.2f;
			}
			else
			{
				velocity.Y += gravity * (float)delta;
			}
			currentState = PLAYER_STATE.ON_WALL;
        }
		else
		{
			velocity.Y += gravity * (float)delta;
            coyoteTime -= delta;
			if(currentState == PLAYER_STATE.ON_WALL)
			{
				currentState = PLAYER_STATE.JUMPING;
			}
        }

        // Get the input direction and handle the movement/deceleration.
        // As good practice, you should replace UI actions with custom gameplay actions.
        Vector2 direction = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");

		// If jump cancelled or intent changes direction cancel midair jump
        if ((Input.IsActionJustReleased("ui_accept") || DirectionOppositeOfIntent(direction)) && currentState == PLAYER_STATE.ROLLING)
        {
            currentState = PLAYER_STATE.FALLING;
        }
        if (currentState != PLAYER_STATE.ROLLING)
		{
			if (direction != Vector2.Zero && currentState != PLAYER_STATE.ROLLING)
			{
				velocity.X = direction.X * Speed;
			}
			else
			{
				velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
			}
			// Mirror position based on last move direction
			if (velocity.X != 0)
			{
				animationNode.Scale = new Vector2(velocity.X > 0 ? 1 : -1, 1);
			}
			if (velocity.X == 0 && currentState == PLAYER_STATE.ON_FLOOR)
			{
				currentState = PLAYER_STATE.IDLE;
			}
		}
        Velocity = velocity;
        SetAnimationBasedOnState();
		previousState = currentState;
		MoveAndSlide();
	}
}
