using Godot;

namespace Firebyte
{
    public partial class Main : Node3D
    {
        public override void _Ready()
        {
            GD.Print("Main Scene Ready");
            
            // Créer le joueur
            var player = new Player();
            player.Name = "Player";
            AddChild(player);
            
            // Créer caméra
            var camera = new Camera3D();
            camera.Name = "Camera3D";
            camera.Position = new Vector3(0, 1.6f, 0);
            player.AddChild(camera);
            
            // Créer sol
            var floor = new MeshInstance3D();
            floor.Mesh = new BoxMesh();
            floor.Position = new Vector3(0, -1, 0);
            ((BoxMesh)floor.Mesh).Size = new Vector3(10, 0.1f, 10);
            AddChild(floor);
            
            // Lumière
            var light = new DirectionalLight3D();
            light.Position = new Vector3(0, 5, 0);
            AddChild(light);
        }
    }
}
