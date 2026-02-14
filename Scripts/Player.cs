using Godot;

namespace Firebyte
{
    public partial class Player : CharacterBody3D
    {
        [Export] public float Speed = 5.0f;
        [Export] public float JumpVelocity = 4.5f;
        
        private Vector3 _velocity = Vector3.Zero;

        public override void _Ready()
        {
            GD.Print("Player Ready");
        }

        public override void _PhysicsProcess(double delta)
        {
            var deltaTime = (float)delta;
            
            // Mouvement de base
            var inputDir = Input.GetVector("move_left", "move_right", "move_forward", "move_backward");
            var direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
            
            if (direction != Vector3.Zero)
            {
                _velocity.X = direction.X * Speed;
                _velocity.Z = direction.Z * Speed;
            }
            else
            {
                _velocity.X = Mathf.MoveToward(_velocity.X, 0, Speed * deltaTime);
                _velocity.Z = Mathf.MoveToward(_velocity.Z, 0, Speed * deltaTime);
            }

            // Saut
            if (Input.IsActionJustPressed("jump") && IsOnFloor())
            {
                _velocity.Y = JumpVelocity;
            }

            // Gravité
            if (!IsOnFloor())
            {
                _velocity.Y -= ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle() * deltaTime;
            }

            Velocity = _velocity;
            MoveAndSlide();
        }
    }
}
