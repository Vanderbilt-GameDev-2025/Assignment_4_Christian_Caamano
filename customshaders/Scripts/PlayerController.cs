using Godot;
using System;
using System.Diagnostics;

public partial class PlayerController : CharacterBody3D
{
	// Gravity
	private const float gravity = 9.8f;

	// Movement logic variables
	private float movementSpeed = 15.0f;
	private float frictionFactor = 60.0f;

	// Jump logic variables
	private bool isGrounded = false;
	private float jumpForce = 7.0f;
	private RayCast3D groundCheck;

	// Camera system variables
	private float mouse_sensitivity = 0.0006f;
	private float twistInput = 0.0f;
	private float pitchInput = 0.0f;
	private Node3D twistPivot;
	private Node3D pitchPivot;

	// Dissolve interaction logic
	[Export]
	private MeshInstance3D targetMesh; // The mesh with the dissolve shader
	private ShaderMaterial shaderMaterial;
	private float dissolveAmount = 0.0f;
	private float dissolveIncrement = 0.1f; // How much to increase dissolve per interaction
	private float maxDissolveAmount = 1.0f;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		groundCheck = GetNode<RayCast3D>("GroundCheck");

		twistPivot = GetNode<Node3D>("TwistPivot");
		pitchPivot = GetNode<Node3D>("TwistPivot/PitchPivot");

		// Capture and hide the mouse during gameplay.
		Input.MouseMode = Input.MouseModeEnum.Captured;

		// Get shader material from the target mesh
		if (targetMesh != null)
		{
			// Try to get the surface material override for surface index 0
			Material surfaceMaterial = targetMesh.GetSurfaceOverrideMaterial(0);
			
			if (surfaceMaterial is ShaderMaterial shaderMat)
			{
				shaderMaterial = shaderMat;
				// Initialize dissolve amount to 0
				UpdateDissolveAmount(0.0f);
			}
			else if (targetMesh.MaterialOverride is ShaderMaterial material)
			{
				// Fall back to MaterialOverride if surface override isn't found
				shaderMaterial = material;
				UpdateDissolveAmount(0.0f);
			}
			else
			{
				GD.PrintErr("Target mesh doesn't have a shader material!");
			}
		}
		else
		{
			GD.PrintErr("Target mesh not assigned!");
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		// Pitch and twist the camera based on user mouse movement.
		twistPivot.RotateY(twistInput);
        pitchPivot.RotateX(pitchInput);
		pitchPivot.Rotation = pitchPivot.Rotation with {
			X = Mathf.Clamp(
			pitchPivot.Rotation.X,
			Mathf.DegToRad(-60),
			Mathf.DegToRad(60))
		};

		// Reset camera system variables for next frame.
		twistInput = 0;
		pitchInput = 0;
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector3 newVel = Velocity;
		
		// Jump logic
		if (!IsOnFloor())
		{
			newVel.Y -= gravity * (float)delta;
		}
		else if (Input.IsActionJustPressed("jump"))
        {
            newVel.Y = jumpForce;
        }

		// Calculate movement right, left, back, or forward based on user input.
		// See project Input Map settings for keyboard mappings.
		Vector3 input = Vector3.Zero;
		input.X = Input.GetAxis("move_left", "move_right");
        input.Z = Input.GetAxis("move_forward", "move_back");

		Vector3 direction = twistPivot.Basis * input;

        if (direction != Vector3.Zero)
        {
            newVel.X = direction.X * movementSpeed;
            newVel.Z = direction.Z * movementSpeed;
        }
        else
        {
            newVel.X = Mathf.MoveToward(Velocity.X, 0, movementSpeed);
            newVel.Z = Mathf.MoveToward(Velocity.Z, 0, movementSpeed);
        }

		// Apply movement.
		Velocity = newVel;
		MoveAndSlide();
	}

	public override void _Input(InputEvent @event)
    {
		if (@event.IsActionPressed("interact"))
        {
			// Increase dissolve amount when interact is pressed
    		IncreaseDissolveAmount();
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        // Player looking around with the mouse
		if (@event is InputEventMouseMotion mouseMotion)
		{
			if (Input.MouseMode == Input.MouseModeEnum.Captured)
			{
				twistInput = -mouseMotion.Relative.X * mouse_sensitivity;
				pitchInput = -mouseMotion.Relative.Y * mouse_sensitivity;
			}
		}
    }

	// Increases the dissolve amount by the dissolve increment
	private void IncreaseDissolveAmount()
	{
		if (shaderMaterial == null) return;
		
		// Increase dissolve amount
		dissolveAmount = Mathf.Min(dissolveAmount + dissolveIncrement, maxDissolveAmount);
		
		// Update the shader parameter
		UpdateDissolveAmount(dissolveAmount);
		
		GD.Print($"Dissolve amount increased to: {dissolveAmount}");
	}
	
	// Updates the dissolve_amount parameter in the shader material
	private void UpdateDissolveAmount(float amount)
	{
		if (shaderMaterial != null)
		{
			shaderMaterial.SetShaderParameter("dissolve_amount", amount);
			
			// Also increase displacement as dissolve increases for a more dramatic effect
			shaderMaterial.SetShaderParameter("displacement_amount", amount * 100.0f);
		}
	}
}
