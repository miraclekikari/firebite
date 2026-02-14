using Godot;
using System;

namespace Firebyte
{
    /// <summary>
    /// Ennemi simplifié compatible Godot 4.2
    /// </summary>
    public partial class Enemy : CharacterBody3D
    {
        // Événements
        [Signal] public delegate void EnemyDiedEventHandler(Enemy enemy);

        // Références aux composants
        private StatsManager _stats;
        private MeshInstance3D _mesh;
        private CollisionShape3D _collisionShape;
        private CpuParticles3D _deathParticles;
        private AudioStreamPlayer3D _deathSound;

        // Configuration de l'ennemi
        [Export] public float MaxHealth { get; set; } = 100.0f;
        [Export] public float MoveSpeed { get; set; } = 3.0f;
        [Export] public float DetectionRange { get; set; } = 15.0f;
        [Export] public float AttackRange { get; set; } = 5.0f;
        [Export] public float AttackDamage { get; set; } = 15.0f;
        [Export] public int XPValue { get; set; } = 50;

        // État interne
        private Vector3 _velocity = Vector3.Zero;
        private bool _isDead = false;
        private Node3D _target;

        // Couleurs
        private readonly Color _enemyColor = new Color(0.8f, 0.2f, 0.2f, 1.0f);

        public override void _Ready()
        {
            GD.Print("👾 Initialisation de l'ennemi...");
            
            // Initialiser les composants
            InitializeComponents();
            InitializeStats();
            InitializeVisuals();
            
            GD.Print("✅ Ennemi initialisé");
        }

        /// <summary>
        /// Initialise les composants de base
        /// </summary>
        private void InitializeComponents()
        {
            // Créer le mesh
            _mesh = new MeshInstance3D();
            _mesh.Name = "Mesh";
            _mesh.Mesh = new CapsuleMesh();
            ((CapsuleMesh)_mesh.Mesh).Height = 2.0f;
            ((CapsuleMesh)_mesh.Mesh).Radius = 0.5f;
            AddChild(_mesh);

            // Créer la collision
            _collisionShape = new CollisionShape3D();
            _collisionShape.Name = "CollisionShape";
            _collisionShape.Shape = new CapsuleShape3D();
            ((CapsuleShape3D)_collisionShape.Shape).Height = 2.0f;
            ((CapsuleShape3D)_collisionShape.Shape).Radius = 0.5f;
            _collisionShape.Position = new Vector3(0, 0, 1.0f);
            AddChild(_collisionShape);

            // Créer les particules de mort
            _deathParticles = new CpuParticles3D();
            _deathParticles.Name = "DeathParticles";
            _deathParticles.Position = new Vector3(0, 1, 0);
            _deathParticles.Emitting = false;
            AddChild(_deathParticles);

            // Créer le son de mort
            _deathSound = new AudioStreamPlayer3D();
            _deathSound.Name = "DeathSound";
            AddChild(_deathSound);
        }

        /// <summary>
        /// Initialise les statistiques
        /// </summary>
        private void InitializeStats()
        {
            _stats = new StatsManager();
            _stats.Name = "StatsManager";
            _stats.MaxHealth = MaxHealth;
            AddChild(_stats);

            // Connecter les signaux
            _stats.Death += OnDeath;
        }

        /// <summary>
        /// Initialise les visuels
        /// </summary>
        private void InitializeVisuals()
        {
            // Matériau de base
            var material = new StandardMaterial3D();
            material.AlbedoColor = _enemyColor;
            material.Metallic = 0.3f;
            material.Roughness = 0.7f;
            _mesh.MaterialOverride = material;

            // Configurer les particules
            SetupDeathParticles();
        }

        /// <summary>
        /// Configure les particules de mort
        /// </summary>
        private void SetupDeathParticles()
        {
            var processMaterial = new ParticleProcessMaterial();
            
            // Configuration des particules
            processMaterial.Direction = Vector3.Up;
            processMaterial.Spread = 45.0f;
            processMaterial.InitialVelocityMin = 2.0f;
            processMaterial.InitialVelocityMax = 5.0f;
            processMaterial.Gravity = Vector3.Down * 9.8f;
            processMaterial.ScaleMin = 0.1f;
            processMaterial.ScaleMax = 0.3f;
            processMaterial.Color = Colors.Red;

            _deathParticles.MaterialOverride = new StandardMaterial3D();
            ((StandardMaterial3D)_deathParticles.MaterialOverride).AlbedoColor = Colors.Red;
            ((StandardMaterial3D)_deathParticles.MaterialOverride).EmissionEnabled = true;
            ((StandardMaterial3D)_deathParticles.MaterialOverride).Emission = Colors.Red;

            _deathParticles.Amount = 50;
            _deathParticles.Lifetime = 2.0f;
            _deathParticles.OneShot = true;
        }

        public override void _PhysicsProcess(double delta)
        {
            if (_isDead) return;

            var deltaTime = (float)delta;

            // Chercher la cible
            FindTarget();

            // Mouvement vers la cible
            if (_target != null)
            {
                MoveTowardsTarget(deltaTime);
            }

            // Appliquer la gravité
            if (!IsOnFloor())
            {
                _velocity.Y -= ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle() * deltaTime;
            }

            // Appliquer le mouvement
            Velocity = _velocity;
            MoveAndSlide();
        }

        /// <summary>
        /// Cherche la cible la plus proche
        /// </summary>
        private void FindTarget()
        {
            var players = GetTree().GetNodesInGroup("player");
            Node3D closestPlayer = null;
            float closestDistance = DetectionRange;

            foreach (Node playerNode in players)
            {
                var player = playerNode as Node3D;
                if (player == null) continue;

                var distance = GlobalPosition.DistanceTo(player.GlobalPosition);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPlayer = player;
                }
            }

            _target = closestPlayer;
        }

        /// <summary>
        /// Se déplace vers la cible
        /// </summary>
        private void MoveTowardsTarget(float deltaTime)
        {
            if (_target == null) return;

            var direction = (_target.GlobalPosition - GlobalPosition).Normalized();
            direction.Y = 0; // Garder le mouvement au sol

            _velocity.X = direction.X * MoveSpeed;
            _velocity.Z = direction.Z * MoveSpeed;

            // Regarder vers la cible
            LookAt(_target.GlobalPosition, Vector3.Up);

            // Attaquer si à portée
            var distance = GlobalPosition.DistanceTo(_target.GlobalPosition);
            if (distance <= AttackRange)
            {
                TryAttack();
            }
        }

        /// <summary>
        /// Tente d'attaquer la cible
        /// </summary>
        private void TryAttack()
        {
            // Logique d'attaque simple
            if (_target is Player player)
            {
                player.TakeDamage(AttackDamage);
                GD.Print($"👾 L'ennemi attaque le joueur! Dégâts: {AttackDamage}");
            }
        }

        /// <summary>
        /// Applique des dégâts à l'ennemi
        /// </summary>
        public void TakeDamage(float damage)
        {
            if (_isDead) return;

            _stats.TakeDamage(damage);
            
            // Effet visuel de dégâts
            ShowDamageEffect();
        }

        /// <summary>
        /// Affiche un effet de dégâts
        /// </summary>
        private void ShowDamageEffect()
        {
            // Flash rouge
            var originalColor = ((StandardMaterial3D)_mesh.MaterialOverride).AlbedoColor;
            ((StandardMaterial3D)_mesh.MaterialOverride).AlbedoColor = Colors.White;

            var timer = GetTree().CreateTimer(0.1f);
            timer.Timeout += () => {
                if (_mesh.MaterialOverride != null)
                {
                    ((StandardMaterial3D)_mesh.MaterialOverride).AlbedoColor = originalColor;
                }
            };
        }

        /// <summary>
        /// Gère la mort de l'ennemi
        /// </summary>
        private void OnDeath()
        {
            if (_isDead) return;

            _isDead = true;
            GD.Print("💀 L'ennemi est mort!");

            // Désactiver la collision
            if (_collisionShape != null)
            {
                _collisionShape.Disabled = true;
            }

            // Effets visuels et sonores
            PlayDeathEffects();

            // Émettre les signaux
            EmitSignal(SignalName.EnemyDied, this);

            // Détruire après un délai
            var destroyTimer = GetTree().CreateTimer(3.0f);
            destroyTimer.Timeout += () => QueueFree();
        }

        /// <summary>
        /// Joue les effets de mort
        /// </summary>
        private void PlayDeathEffects()
        {
            // Particules de mort
            if (_deathParticles != null)
            {
                _deathParticles.Emitting = true;
            }

            // Son de mort
            if (_deathSound != null)
            {
                // Créer un son simple si aucun n'est assigné
                if (_deathSound.Stream == null)
                {
                    var audioGenerator = new AudioStreamGenerator();
                    audioGenerator.BufferLength = 0.5f;
                    audioGenerator.MixRate = 44100;
                    _deathSound.Stream = audioGenerator;
                }
                _deathSound.Play();
            }

            // Animation de mort
            var tween = CreateTween();
            tween.TweenProperty(this, "scale", Vector3.Zero, 1.0f);
            tween.SetEase(Tween.EaseType.In);
            tween.SetTrans(Tween.TransitionType.Back);
        }

        /// <summary>
        /// Nettoie les ressources
        /// </summary>
        public override void _ExitTree()
        {
            GD.Print("🧹 Nettoyage de l'ennemi...");
            
            // Nettoyer les références
            _stats = null;
            _mesh = null;
            _collisionShape = null;
            _deathParticles = null;
            _deathSound = null;
            _target = null;
        }
    }
}
