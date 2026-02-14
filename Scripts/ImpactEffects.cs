using Godot;
using System;

namespace Firebyte
{
    /// <summary>
    /// Gestionnaire d'effets visuels simplifié compatible Godot 4.2
    /// </summary>
    public partial class ImpactEffects : Node3D
    {
        // Références aux ressources
        [Export] public PackedScene SparkEffectScene { get; set; }
        [Export] public PackedScene SmokeEffectScene { get; set; }
        [Export] public AudioStream ImpactSound { get; set; }

        // Cache d'effets pour optimisation
        private CpuParticles3D _sparkPool;
        private CpuParticles3D _smokePool;
        private AudioStreamPlayer3D _soundPool;

        public override void _Ready()
        {
            GD.Print("💥 Initialisation des effets d'impact...");
            
            InitializeEffects();
            
            GD.Print("✅ Effets d'impact initialisés");
        }

        /// <summary>
        /// Initialise les pools d'effets
        /// </summary>
        private void InitializeEffects()
        {
            // Créer les pools
            _sparkPool = CreateSparkEffect();
            _smokePool = CreateSmokeEffect();
            _soundPool = CreateSoundEffect();
            
            // Ajouter comme enfants
            AddChild(_sparkPool);
            AddChild(_smokePool);
            AddChild(_soundPool);
        }

        /// <summary>
        /// Crée un effet d'étincelles
        /// </summary>
        private CpuParticles3D CreateSparkEffect()
        {
            var particles = new CpuParticles3D();
            particles.Name = "SparkEffect";
            particles.OneShot = true;
            particles.Amount = 30;
            particles.Lifetime = 0.5f;
            particles.Emitting = false;

            var material = new ParticleProcessMaterial();
            material.Direction = Vector3.Up;
            material.Spread = 180.0f;
            material.InitialVelocityMin = 2.0f;
            material.InitialVelocityMax = 5.0f;
            material.Gravity = Vector3.Down * 9.8f;
            material.ScaleMin = 0.02f;
            material.ScaleMax = 0.05f;
            material.Color = Colors.Yellow;

            particles.MaterialOverride = new StandardMaterial3D();
            ((StandardMaterial3D)particles.MaterialOverride).AlbedoColor = Colors.Yellow;
            ((StandardMaterial3D)particles.MaterialOverride).EmissionEnabled = true;
            ((StandardMaterial3D)particles.MaterialOverride).Emission = Colors.Yellow;

            return particles;
        }

        /// <summary>
        /// Crée un effet de fumée
        /// </summary>
        private CpuParticles3D CreateSmokeEffect()
        {
            var particles = new CpuParticles3D();
            particles.Name = "SmokeEffect";
            particles.OneShot = true;
            particles.Amount = 20;
            particles.Lifetime = 2.0f;
            particles.Emitting = false;

            var material = new ParticleProcessMaterial();
            material.Direction = Vector3.Up;
            material.Spread = 45.0f;
            material.InitialVelocityMin = 0.5f;
            material.InitialVelocityMax = 1.5f;
            material.Gravity = Vector3.Up * 0.5f;
            material.ScaleMin = 0.1f;
            material.ScaleMax = 0.3f;
            material.Color = new Color(0.5f, 0.5f, 0.5f, 0.3f);

            particles.MaterialOverride = new StandardMaterial3D();
            ((StandardMaterial3D)particles.MaterialOverride).AlbedoColor = new Color(0.5f, 0.5f, 0.5f, 0.3f);
            ((StandardMaterial3D)particles.MaterialOverride).Transparency = BaseMaterial3D.TransparencyEnum.Alpha;

            return particles;
        }

        /// <summary>
        /// Crée un effet sonore
        /// </summary>
        private AudioStreamPlayer3D CreateSoundEffect()
        {
            var sound = new AudioStreamPlayer3D();
            sound.Name = "ImpactSound";
            sound.MaxDistance = 20.0f;

            // Créer un son simple si aucun n'est assigné
            if (sound.Stream == null)
            {
                var audioGenerator = new AudioStreamGenerator();
                audioGenerator.BufferLength = 0.1f;
                audioGenerator.MixRate = 44100;
                sound.Stream = audioGenerator;
            }

            return sound;
        }

        /// <summary>
        /// Joue un effet d'impact à la position spécifiée
        /// </summary>
        public void PlayImpact(Vector3 position, Vector3 normal)
        {
            GD.Print($"💥 Impact à position {position}");

            // Créer les effets
            CreateImpactAt(position, normal);
        }

        /// <summary>
        /// Crée les effets d'impact
        /// </summary>
        private void CreateImpactAt(Vector3 position, Vector3 normal)
        {
            // Effets d'étincelles
            CreateSparkBurst(position, normal);

            // Effets de fumée
            CreateSmokePuff(position, normal);

            // Effet sonore
            PlayImpactSound(position);
        }

        /// <summary>
        /// Crée une explosion d'étincelles
        /// </summary>
        private void CreateSparkBurst(Vector3 position, Vector3 normal)
        {
            var sparks = _sparkPool.Duplicate() as CpuParticles3D;
            sparks.Position = position;
            sparks.Rotation = new Vector3(
                Mathf.Atan2(normal.Z, normal.Y),
                Mathf.Atan2(normal.X, normal.Z),
                0
            );
            
            GetTree().CurrentScene.AddChild(sparks);
            sparks.Emitting = true;

            // Nettoyer après l'animation
            var timer = GetTree().CreateTimer(2.0f);
            timer.Timeout += () => sparks.QueueFree();
        }

        /// <summary>
        /// Crée une bouffée de fumée
        /// </summary>
        private void CreateSmokePuff(Vector3 position, Vector3 normal)
        {
            var smoke = _smokePool.Duplicate() as CpuParticles3D;
            smoke.Position = position + normal * 0.1f;
            
            GetTree().CurrentScene.AddChild(smoke);
            smoke.Emitting = true;

            // Nettoyer après l'animation
            var timer = GetTree().CreateTimer(3.0f);
            timer.Timeout += () => smoke.QueueFree();
        }

        /// <summary>
        /// Joue un son d'impact
        /// </summary>
        private void PlayImpactSound(Vector3 position)
        {
            var sound = _soundPool.Duplicate() as AudioStreamPlayer3D;
            sound.Position = position;
            
            GetTree().CurrentScene.AddChild(sound);
            sound.Play();

            // Nettoyer après le son
            var timer = GetTree().CreateTimer(1.0f);
            timer.Timeout += () => sound.QueueFree();
        }

        /// <summary>
        /// Nettoie les ressources
        /// </summary>
        public override void _ExitTree()
        {
            GD.Print("🧹 Nettoyage des effets d'impact...");
            
            // Nettoyer les pools
            _sparkPool?.QueueFree();
            _smokePool?.QueueFree();
            _soundPool?.QueueFree();
        }
    }
}
