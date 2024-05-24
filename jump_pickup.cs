using Godot;
using System;

[Tool]
public partial class jump_pickup : Node2D
{
	[Signal]
	public delegate void AbilityPickupEventHandler(Vector2 direction);
	public Vector2 ComputeDirection()
	{
		return new Vector2(Mathf.Cos(Rotation), Mathf.Sin(Rotation));
	}
	// Called when the node enters the scene tree for the first time.

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	public void _on_area_2d_body_entered(PhysicsBody2D area)
	{
        if (area.GetClass() == "CharacterBody2D")
		{
			GetParent().GetNode<Player>("Player").OnAbilityPickup(ComputeDirection());
        }

    }
}
