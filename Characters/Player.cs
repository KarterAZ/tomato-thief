using Godot;
using System;

public partial class Player : CharacterBody2D
{
	[Export] public float maxJumpHeight = 72f;
	[Export] public float timeToPeak = 0.4f;
	[Export] public float timeToFall = 0.35f;
	[Export] public float runSpeed = 250;
	[Export] public float dashSpeed = 400;
	[Export] public float dashDuration = 0.2f;
	[Export] public float dashCooldown = 0.35f;

	private float jumpVelocity;
	private float jumpGravity;
	private float fallGravity;
	private float dashTime = 0.0f;
	private float dashCDTimer = 0.0f;
	private int wasOnFloor = 0;
	private bool isDashing = false;
	private bool dashOnCD = false;
	
	private AnimatedSprite2D animatedSprite2D;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		animatedSprite2D = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		animatedSprite2D.Play();

		float v0Y = 2 * maxJumpHeight / timeToPeak;
		float gAsc = 2 * maxJumpHeight / (timeToPeak * timeToPeak);
		float gDes = 2 * maxJumpHeight / (timeToFall * timeToFall);

		// We adjust the directions for Godot's system, which uses positive Y values pointing down
		jumpVelocity = -v0Y; // up (-Y)
		jumpGravity = gAsc; // down (+Y)
		fallGravity = gDes; // down (+Y)
	}

	private void GetInput(float delta)
	{
		var velocity = Velocity;

		var right = Input.IsActionPressed("move_right");
		var left = Input.IsActionPressed("move_left");
		var jump = Input.IsActionPressed("jump");
		var dash = Input.IsActionPressed("dash");

		if(isDashing)
		{
			dashTime -= delta;
			if(dashTime < 0f)
			{
				isDashing = false;
				dashOnCD = true;
				dashCDTimer = dashCooldown;
			}
		}
		else if(!isDashing && !dashOnCD && dash)
		{
			isDashing = true;
			dashTime = dashDuration;
			velocity.Y = 0f;
			if (right)
			{
				velocity.X += dashSpeed;
				animatedSprite2D.FlipH = false;
			}
			else if (left)
			{
				velocity.X -= dashSpeed;
				animatedSprite2D.FlipH = true;
			}
			else
			{
				velocity.X = animatedSprite2D.FlipH ? dashSpeed : -dashSpeed;
			}
		}
		else if(!isDashing)
		{
			velocity.X = 0;

			float gravity = velocity.Y < 0f ? jumpGravity : fallGravity;

			if(dashOnCD)
			{
				dashCDTimer -= delta;
				if(dashCDTimer < 0)
				{
					dashOnCD = !IsOnFloor();
				}
			}

			//no grav when grounded
			if(!IsOnFloor())
			{
				velocity.Y += gravity * delta;
			}
			else
			{
				velocity.Y = 0f;
			}
			//do the movements
			if ((IsOnFloor() || wasOnFloor > 0) && jump)
			{
				velocity.Y = jumpVelocity;
			}
			if (right)
			{
				velocity.X += runSpeed;
				animatedSprite2D.FlipH = false;
			}
			else if (left)
			{
				velocity.X -= runSpeed;
				animatedSprite2D.FlipH = true;
			}
		}

		Velocity = velocity;
	}

	private void SetAnimation()
	{
		var velocity = Velocity;

		if(isDashing)
		{
			animatedSprite2D.Animation = "dash";
		}
		else
		{
			if (velocity.Length() > 0)
			{
				if(velocity.X != 0 && IsOnFloor())
				{
					animatedSprite2D.Animation = "run";
				}
				if(velocity.Y != 0 && !IsOnFloor())
				{
					animatedSprite2D.Animation = "jump";
				}
			}
			else
			{
				animatedSprite2D.Animation = "idle";
			}
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		//extra jump frames
		if(IsOnFloor())
		{
			wasOnFloor = 4;
		}
		else if(wasOnFloor > 0)
		{
			wasOnFloor--;
		}

		GetInput((float)delta);
		SetAnimation();
		MoveAndSlide();
	}
}
