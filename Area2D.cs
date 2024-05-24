using Godot;
using System;
using System.Security.Cryptography;

public partial class Area2D : Godot.Area2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        GD.Print("blahbalhbahlbahl");
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}
	public void _on_body_entered(PhysicsBody2D area)
	{
		if(area.GetClass() == "TileMap")
		{
			return;
		}
		GetTree().ReloadCurrentScene();
	}
}
