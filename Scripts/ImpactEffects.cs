using Godot;

namespace Firebyte
{
    /// <summary>
    /// Gestionnaire d'effets d'impact pour les tirs
    /// </summary>
    public partial class ImpactEffects : Node3D
    {
        // Références aux scènes d'effets
        [Export] public PackedScene SparkScene { get; set; }
        [Export] public PackedScene DecalScene { get; set; }
        [Export] public PackedScene SmokeScene { get; set; }

        // Paramètres des effets
        [Export] public float SparkLifetime { get; set; } = 2.0f;
        [Export] public float DecalLifetime { get; set; } = 10.0f;
        [Export] public float SmokeLifetime { get; set; } = 3.0f;

        public override void _Ready()
        {
            GD.Print("⚡ ImpactEffects initialisé");
        }

        /// <summary>
        /// Crée des effets d'impact à une position
        /// </summary>
        public void CreateImpact(Vector3 position, Vector3 normal, string surfaceType = "metal")
        {
            GD.Print($"💥 Création d'impact: position={position}, normal={normal}, surface={surfaceType}");
            
            // Étincelles (sparks)
            CreateSparks(position, normal);
            
            // Marque d'impact (decal)
            CreateDecal(position, normal, surfaceType);
            
            // Fumée
            CreateSmoke(position, normal);
            
            // Son d'impact
            PlayImpactSound(position, surfaceType);
        }

        /// <summary>
        /// Crée des étincelles à l'impact
        /// </summary>
        private void CreateSparks(Vector3 position, Vector3 normal)
        {
            var sparks = new CpuParticles3D();
            sparks.Name = "ImpactSparks";
            sparks.Position = position;
            sparks.Amount = 30;
            sparks.Lifetime = SparkLifetime;
            sparks.Emitting = true;
            sparks.OneShot = true;
            
            var processMaterial = new ParticleProcessMaterial();
            processMaterial.Direction = normal;
            processMaterial.Spread = 45.0f;
            processMaterial.InitialVelocityMin = 2.0f;
            processMaterial.InitialVelocityMax = 8.0f;
            processMaterial.Gravity = Vector3.Down * 9.8f;
            processMaterial.ScaleMin = 0.02f;
            processMaterial.ScaleMax = 0.08f;
            processMaterial.Color = new Color(1.0f, 0.8f, 0.2f); // Orange
            processMaterial.Emission = new Color(1.0f, 0.5f, 0.0f);
            processMaterial.EmissionEnergy = 3.0f;
            
            sparks.ProcessMaterial = processMaterial;
            
            // Ajouter à la scène
            GetTree().CurrentScene.AddChild(sparks);
            
            // Détruire après la durée de vie
            var timer = GetTree().CreateTimer(SparkLifetime + 1.0f);
            timer.Timeout += () => sparks.QueueFree();
        }

        /// <summary>
        /// Crée une marque d'impact sur la surface
        /// </summary>
        private void CreateDecal(Vector3 position, Vector3 normal, string surfaceType)
        {
            var decal = new Sprite3D();
            decal.Name = "ImpactDecal";
            decal.Position = position;
            
            // Orienter le décal selon la normale
            decal.LookAt(position + normal, Vector3.Up);
            
            // Texture du décal selon le type de surface
            var texture = GetDecalTexture(surfaceType);
            decal.Texture = texture;
            
            // Taille et transparence
            decal.PixelSize = 0.01f;
            decal.Transparency = 0.7f;
            decal.Billboard = BaseMaterial3D.BillboardModeEnum.Enabled;
            
            // Matériau pour le décal
            var material = new StandardMaterial3D();
            material.Transparency = BaseMaterial3D.TransparencyEnum.Alpha;
            material.AlbedoColor = new Color(0.3f, 0.3f, 0.3f, 0.7f);
            material.NoDepthTest = true;
            decal.MaterialOverride = material;
            
            GetTree().CurrentScene.AddChild(decal);
            
            // Détruire après la durée de vie
            var timer = GetTree().CreateTimer(DecalLifetime);
            timer.Timeout += () => {
                // Animation de fondu
                var fadeTween = decal.CreateTween();
                fadeTween.TweenProperty(decal, "transparency", 1.0f, 1.0f);
                fadeTween.TweenCallback(Callable.From(() => decal.QueueFree()));
            };
        }

        /// <summary>
        /// Crée de la fumée à l'impact
        /// </summary>
        private void CreateSmoke(Vector3 position, Vector3 normal)
        {
            var smoke = new CpuParticles3D();
            smoke.Name = "ImpactSmoke";
            smoke.Position = position + normal * 0.1f; // Léger décalage pour éviter le Z-fighting
            smoke.Amount = 20;
            smoke.Lifetime = SmokeLifetime;
            smoke.Emitting = true;
            smoke.OneShot = true;
            
            var processMaterial = new ParticleProcessMaterial();
            processMaterial.Direction = normal;
            processMaterial.Spread = 30.0f;
            processMaterial.InitialVelocityMin = 0.5f;
            processMaterial.InitialVelocityMax = 2.0f;
            processMaterial.Gravity = Vector3.Up * 0.5f; // Fumée qui monte
            processMaterial.ScaleMin = 0.1f;
            processMaterial.ScaleMax = 0.5f;
            processMaterial.Color = new Color(0.5f, 0.5f, 0.5f, 0.3f); // Gris clair
            processMaterial.Emission = new Color(0.8f, 0.8f, 0.8f);
            processMaterial.EmissionEnergy = 0.5f;
            
            smoke.ProcessMaterial = processMaterial;
            
            GetTree().CurrentScene.AddChild(smoke);
            
            // Détruire après la durée de vie
            var timer = GetTree().CreateTimer(SmokeLifetime + 1.0f);
            timer.Timeout += () => smoke.QueueFree();
        }

        /// <summary>
        /// Joue un son d'impact
        /// </summary>
        private void PlayImpactSound(Vector3 position, string surfaceType)
        {
            var audioPlayer = new AudioStreamPlayer3D();
            audioPlayer.Name = "ImpactSound";
            audioPlayer.Position = position;
            
            // Générer un son simple si aucun n'est disponible
            var audioGenerator = new AudioStreamGenerator();
            audioGenerator.BufferLength = 0.1f;
            audioGenerator.MixRate = 44100;
            audioPlayer.Stream = audioGenerator;
            
            // Volume selon le type de surface
            audioPlayer.VolumeDb = surfaceType switch
            {
                "metal" => -10.0f,
                "concrete" => -5.0f,
                "flesh" => -15.0f,
                _ => -8.0f
            };
            
            GetTree().CurrentScene.AddChild(audioPlayer);
            audioPlayer.Play();
            
            // Détruire après la lecture
            audioPlayer.Finished += () => audioPlayer.QueueFree();
        }

        /// <summary>
        /// Obtient la texture du décal selon le type de surface
        /// </summary>
        private Texture2D GetDecalTexture(string surfaceType)
        {
            // Créer une texture simple par défaut
            var image = new Image();
            image.Create(64, 64, false, Image.Format.Rgba8);
            
            // Dessiner un cercle pour l'impact
            var center = new Vector2I(32, 32);
            for (int x = 0; x < 64; x++)
            {
                for (int y = 0; y < 64; y++)
                {
                    var distance = Vector2I.Distance(new Vector2I(x, y), center);
                    if (distance < 20)
                    {
                        var alpha = (byte)(255 * (1.0f - distance / 20.0f));
                        var color = surfaceType switch
                        {
                            "metal" => new Color(0.7f, 0.7f, 0.8f, alpha / 255.0f),
                            "concrete" => new Color(0.5f, 0.5f, 0.5f, alpha / 255.0f),
                            "flesh" => new Color(0.8f, 0.2f, 0.2f, alpha / 255.0f),
                            _ => new Color(0.3f, 0.3f, 0.3f, alpha / 255.0f)
                        };
                        image.SetPixel(x, y, color);
                    }
                }
            }
            
            return ImageTexture.CreateFromImage(image);
        }

        /// <summary>
        /// Crée des effets d'impact spéciaux pour les tirs critiques
        /// </summary>
        public void CreateCriticalImpact(Vector3 position, Vector3 normal)
        {
            GD.Print("⚡ Impact critique!");
            
            // Effets standards plus intenses
            CreateImpact(position, normal, "metal");
            
            // Étincelles supplémentaires
            var criticalSparks = new GPUParticles3D();
            criticalSparks.Position = position;
            criticalSparks.Amount = 50;
            criticalSparks.Lifetime = 3.0f;
            criticalSparks.Emitting = true;
            criticalSparks.OneShot = true;
            
            var processMaterial = new ParticleProcessMaterial();
            processMaterial.Direction = normal;
            processMaterial.Spread = 60.0f;
            processMaterial.InitialVelocityMin = 5.0f;
            processMaterial.InitialVelocityMax = 15.0f;
            processMaterial.Gravity = Vector3.Down * 9.8f;
            processMaterial.ScaleMin = 0.05f;
            processMaterial.ScaleMax = 0.15f;
            processMaterial.Color = new Color(1.0f, 1.0f, 0.0f); // Jaune pour critique
            processMaterial.Emission = new Color(1.0f, 1.0f, 0.0f);
            processMaterial.EmissionEnergy = 5.0f;
            
            criticalSparks.ProcessMaterial = processMaterial;
            GetTree().CurrentScene.AddChild(criticalSparks);
            
            // Détruire après
            var timer = GetTree().CreateTimer(4.0f);
            timer.Timeout += () => criticalSparks.QueueFree();
        }

        /// <summary>
        /// Crée des effets d'impact pour les tirs enragés
        /// </summary>
        public void CreateBerserkImpact(Vector3 position, Vector3 normal)
        {
            GD.Print("🔥 Impact enragé!");
            
            // Effets standards avec des couleurs différentes
            CreateImpact(position, normal, "metal");
            
            // Flammes
            var flames = new GPUParticles3D();
            flames.Position = position;
            flames.Amount = 25;
            flames.Lifetime = 2.0f;
            flames.Emitting = true;
            flames.OneShot = true;
            
            var processMaterial = new ParticleProcessMaterial();
            processMaterial.Direction = normal;
            processMaterial.Spread = 90.0f;
            processMaterial.InitialVelocityMin = 1.0f;
            processMaterial.InitialVelocityMax = 4.0f;
            processMaterial.Gravity = Vector3.Up * 0.2f; // Flammes qui montent
            processMaterial.ScaleMin = 0.1f;
            processMaterial.ScaleMax = 0.3f;
            processMaterial.Color = new Color(1.0f, 0.3f, 0.0f); // Rouge-orange
            processMaterial.Emission = new Color(1.0f, 0.5f, 0.0f);
            processMaterial.EmissionEnergy = 4.0f;
            
            flames.ProcessMaterial = processMaterial;
            GetTree().CurrentScene.AddChild(flames);
            
            // Détruire après
            var timer = GetTree().CreateTimer(3.0f);
            timer.Timeout += () => flames.QueueFree();
        }
    }
}
