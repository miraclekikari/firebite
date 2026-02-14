using Godot;
using System;

namespace Firebyte
{
    /// <summary>
    /// Ennemi avec système de loot et effets visuels
    /// </summary>
    public partial class Enemy : CharacterBody3D
    {
        // Événements
        [Signal] public delegate void EnemyDiedEventHandler(Enemy enemy);
        [Signal] public delegate void LootDroppedEventHandler(Vector3 position, LootRarity rarity);

        // Types de loot
        public enum LootRarity
        {
            Common,    // 60% chance - Vert
            Rare,      // 30% chance - Bleu  
            Epic       // 10% chance - Violet
        }

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
        [Export] public PackedScene LootBoxScene { get; set; }

        // État interne
        private Vector3 _velocity = Vector3.Zero;
        private bool _isDead = false;
        private Node3D _target;

        // Couleurs par rareté
        private readonly Color _commonColor = new Color(0.0f, 1.0f, 0.0f);    // Vert
        private readonly Color _rareColor = new Color(0.0f, 0.5f, 1.0f);   // Bleu
        private readonly Color _epicColor = new Color(0.8f, 0.0f, 1.0f);   // Violet

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
            material.AlbedoColor = new Color(0.8f, 0.2f, 0.2f); // Rouge foncé
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
            processMaterial.Color = _rareColor;
            processMaterial.Emission = Color.White;
            processMaterial.EmissionEnergy = 2.0f;

            _deathParticles.ProcessMaterial = processMaterial;
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

            // Laisser tomber le loot
            DropLoot();

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
                
                // Changer la couleur selon la rareté du loot
                var rarity = DetermineLootRarity();
                var processMaterial = (ParticleProcessMaterial)_deathParticles.ProcessMaterial;
                
                switch (rarity)
                {
                    case LootRarity.Common:
                        processMaterial.Color = _commonColor;
                        break;
                    case LootRarity.Rare:
                        processMaterial.Color = _rareColor;
                        break;
                    case LootRarity.Epic:
                        processMaterial.Color = _epicColor;
                        break;
                }
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
        /// Fait tomber le loot
        /// </summary>
        private void DropLoot()
        {
            var rarity = DetermineLootRarity();
            var position = GlobalPosition + Vector3.Up; // Léger décalage en hauteur

            GD.Print($"🎁 Loot droppé: {rarity} à position {position}");

            // Créer la boîte de loot
            if (LootBoxScene != null)
            {
                var lootBox = LootBoxScene.Instantiate();
                lootBox.Position = position;
                
                // Configurer la rareté
                if (lootBox.HasMethod("SetRarity"))
                {
                    lootBox.Call("SetRarity", rarity);
                }

                GetTree().CurrentScene.AddChild(lootBox);
            }
            else
            {
                // Créer une boîte de loot par défaut
                CreateDefaultLootBox(position, rarity);
            }

            // Émettre le signal
            EmitSignal(SignalName.LootDropped, position, rarity);
        }

        /// <summary>
        /// Détermine la rareté du loot
        /// </summary>
        private LootRarity DetermineLootRarity()
        {
            var random = new Random();
            var roll = random.NextDouble();

            if (roll < 0.6) return LootRarity.Common;    // 60%
            if (roll < 0.9) return LootRarity.Rare;      // 30%
            return LootRarity.Epic;                           // 10%
        }

        /// <summary>
        /// Crée une boîte de loot par défaut
        /// </summary>
        private void CreateDefaultLootBox(Vector3 position, LootRarity rarity)
        {
            var lootBox = new MeshInstance3D();
            lootBox.Name = "LootBox";
            lootBox.Position = position;
            lootBox.Mesh = new BoxMesh();
            ((BoxMesh)lootBox.Mesh).Size = new Vector3(0.5f, 0.5f, 0.5f);

            // Matériau selon la rareté
            var material = new StandardMaterial3D();
            material.EmissionEnabled = true;
            
            switch (rarity)
            {
                case LootRarity.Common:
                    material.AlbedoColor = _commonColor;
                    material.Emission = _commonColor;
                    break;
                case LootRarity.Rare:
                    material.AlbedoColor = _rareColor;
                    material.Emission = _rareColor;
                    break;
                case LootRarity.Epic:
                    material.AlbedoColor = _epicColor;
                    material.Emission = _epicColor;
                    break;
            }

            material.Metallic = 0.8f;
            material.Roughness = 0.2f;
            lootBox.MaterialOverride = material;

            // Ajouter une collision pour la collection
            var collisionShape = new CollisionShape3D();
            collisionShape.Shape = new BoxShape3D();
            collisionShape.Position = new Vector3(0, 0.25f, 0);
            lootBox.AddChild(collisionShape);

            // Ajouter un script de collecte
            var lootScript = new LootBox();
            lootScript.Rarity = rarity;
            lootBox.AddChild(lootScript);

            GetTree().CurrentScene.AddChild(lootBox);

            // Animation de spawn
            lootBox.Scale = Vector3.Zero;
            var tween = CreateTween();
            tween.TweenProperty(lootBox, "scale", Vector3.One, 0.3f);
            tween.SetEase(Tween.EaseType.Out);
            tween.SetTrans(Tween.TransitionType.Back);
        }

        /// <summary>
        /// Obtient la couleur selon la rareté
        /// </summary>
        public Color GetRarityColor(LootRarity rarity)
        {
            return rarity switch
            {
                LootRarity.Common => _commonColor,
                LootRarity.Rare => _rareColor,
                LootRarity.Epic => _epicColor,
                _ => Colors.White
            };
        }
    }

    /// <summary>
    /// Script pour les boîtes de loot
    /// </summary>
    public partial class LootBox : Node3D
    {
        [Export] public Enemy.LootRarity Rarity { get; set; } = Enemy.LootRarity.Common;
        
        private bool _isCollected = false;
        private float _rotationSpeed = 2.0f;

        public override void _Ready()
        {
            // Ajouter au groupe "loot" pour la détection
            AddToGroup("loot");
        }

        public override void _Process(double delta)
        {
            if (_isCollected) return;

            // Rotation de la boîte
            RotateY((float)delta * _rotationSpeed);

            // Vérifier si le joueur est proche
            var players = GetTree().GetNodesInGroup("player");
            foreach (Node playerNode in players)
            {
                var player = playerNode as Node3D;
                if (player == null) continue;

                var distance = GlobalPosition.DistanceTo(player.GlobalPosition);
                if (distance < 2.0f) // Distance de collecte
                {
                    Collect(player);
                    break;
                }
            }
        }

        /// <summary>
        /// Collecte le loot
        /// </summary>
        private void Collect(Node3D collector)
        {
            if (_isCollected) return;

            _isCollected = true;
            GD.Print($"🎁 Loot collecté: {Rarity} par {collector.Name}");

            // Donner des bonus selon la rareté
            if (collector is Player player)
            {
                switch (Rarity)
                {
                    case Enemy.LootRarity.Common:
                        player.Heal(25);
                        player.AddXP(25);
                        break;
                    case Enemy.LootRarity.Rare:
                        player.Heal(50);
                        player.AddXP(75);
                        break;
                    case Enemy.LootRarity.Epic:
                        player.Heal(100);
                        player.AddXP(150);
                        break;
                }
            }

            // Effet de collecte
            PlayCollectEffect();

            // Détruire la boîte
            QueueFree();
        }

        /// <summary>
        /// Joue l'effet de collecte
        /// </summary>
        private void PlayCollectEffect()
        {
            // Particules de collecte
            var particles = new CpuParticles3D();
            particles.Position = GlobalPosition;
            particles.OneShot = true;
            particles.Amount = 20;
            particles.Lifetime = 1.0f;

            var processMaterial = new ParticleProcessMaterial();
            processMaterial.Direction = Vector3.Up;
            processMaterial.Spread = 180.0f;
            processMaterial.InitialVelocityMin = 1.0f;
            processMaterial.InitialVelocityMax = 3.0f;
            processMaterial.ScaleMin = 0.05f;
            processMaterial.ScaleMax = 0.2f;
            
            // Couleur selon la rareté
            var enemy = new Enemy();
            processMaterial.Color = enemy.GetRarityColor(Rarity);
            processMaterial.Emission = processMaterial.Color;
            processMaterial.EmissionEnergy = 3.0f;

            particles.ProcessMaterial = processMaterial;
            GetTree().CurrentScene.AddChild(particles);
            particles.Emitting = true;

            // Détruire après l'animation
            var timer = GetTree().CreateTimer(2.0f);
            timer.Timeout += () => particles.QueueFree();
        }
    }
}
