using Godot;
using System;

namespace Firebyte
{
    /// <summary>
    /// Joueur principal compatible Godot 4.2
    /// </summary>
    public partial class Player : CharacterBody3D
    {
        // Références aux composants
        private Camera3D _camera;
        private UI _gameUI;
        private WeaponManager _weaponManager;
        private CameraShake _cameraShake;
        private ImpactEffects _impactEffects;
        private StatsManager _stats;

        // Paramètres de mouvement
        [Export] public float Speed { get; set; } = 5.0f;
        [Export] public float Acceleration { get; set; } = 20.0f;
        [Export] public float Friction { get; set; } = 10.0f;
        [Export] public float JumpVelocity { get; set; } = 4.5f;

        // Variables internes
        private Vector3 _velocity = Vector3.Zero;

        public override void _Ready()
        {
            GD.Print("🎮 Initialisation du joueur...");
            
            InitializeComponents();
            SetupCamera();
            SetupInput();
            
            GD.Print("✅ Joueur initialisé");
        }

        /// <summary>
        /// Initialise les composants du joueur
        /// </summary>
        private void InitializeComponents()
        {
            // Créer la caméra
            _camera = new Camera3D();
            _camera.Name = "Camera3D";
            _camera.Position = new Vector3(0, 1.6f, 0);
            AddChild(_camera);

            // Créer les gestionnaires
            _gameUI = new UI();
            _gameUI.Name = "UI";
            AddChild(_gameUI);

            _weaponManager = new WeaponManager();
            _weaponManager.Name = "WeaponManager";
            AddChild(_weaponManager);

            _cameraShake = new CameraShake();
            _cameraShake.Name = "CameraShake";
            _camera.AddChild(_cameraShake);

            _impactEffects = new ImpactEffects();
            _impactEffects.Name = "ImpactEffects";
            AddChild(_impactEffects);

            _stats = new StatsManager();
            _stats.Name = "StatsManager";
            AddChild(_stats);

            // Connecter les signaux
            ConnectSignals();
        }

        /// <summary>
        /// Configure la caméra
        /// </summary>
        private void SetupCamera()
        {
            if (_camera != null)
            {
                _camera.Current = true;
            }
        }

        /// <summary>
        /// Configure les entrées
        /// </summary>
        private void SetupInput()
        {
            Input.MouseMode = Input.MouseModeEnum.Captured;
        }

        /// <summary>
        /// Connecte les signaux
        /// </summary>
        private void ConnectSignals()
        {
            if (_stats != null)
            {
                _stats.HealthChanged += (current, max) => _gameUI?.UpdateHealth(current, max);
                _stats.EnergyChanged += (current, max) => _gameUI?.UpdateEnergy(current, max);
                _stats.XPChanged += (current, max) => _gameUI?.UpdateXP(current, max);
                _stats.PlayerLevelUp += (level) => _gameUI?.UpdateLevel(level);
            }

            if (_weaponManager != null)
            {
                _weaponManager.AmmoChanged += (current, max) => _gameUI?.UpdateAmmo(current, max);
            }
        }

        public override void _PhysicsProcess(double delta)
        {
            var deltaTime = (float)delta;
            
            HandleMovement(deltaTime);
            HandleInput();
            
            // Appliquer la gravité
            if (!IsOnFloor())
            {
                _velocity.Y -= ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle() * deltaTime;
            }
            else
            {
                _velocity.Y = 0;
            }

            // Appliquer le mouvement
            Velocity = _velocity;
            MoveAndSlide();
            
            UpdateUI();
        }

        /// <summary>
        /// Gère le mouvement du joueur
        /// </summary>
        private void HandleMovement(float deltaTime)
        {
            var inputDir = Input.GetVector("move_left", "move_right", "move_forward", "move_backward");
            var direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
            
            if (direction != Vector3.Zero)
            {
                _velocity.X = direction.X * Speed;
                _velocity.Z = direction.Z * Speed;
            }
            else
            {
                _velocity.X = Mathf.MoveToward(_velocity.X, 0, Friction * deltaTime);
                _velocity.Z = Mathf.MoveToward(_velocity.Z, 0, Friction * deltaTime);
            }
        }

        /// <summary>
        /// Gère les entrées du joueur
        /// </summary>
        private void HandleInput()
        {
            // Saut
            if (Input.IsActionJustPressed("jump") && IsOnFloor())
            {
                _velocity.Y = JumpVelocity;
            }

            // Tir
            if (Input.IsActionJustPressed("shoot"))
            {
                Shoot();
            }

            // Rechargement
            if (Input.IsActionJustPressed("reload"))
            {
                Reload();
            }
        }

        /// <summary>
        /// Met à jour l'interface
        /// </summary>
        private void UpdateUI()
        {
            if (_gameUI != null && _stats != null)
            {
                _gameUI.UpdateHealth(_stats.CurrentHealth, _stats.MaxHealth);
                _gameUI.UpdateEnergy(_stats.CurrentEnergy, _stats.MaxEnergy);
                _gameUI.UpdateXP(_stats.CurrentXP, _stats.XPToNextLevel);
                _gameUI.UpdateLevel(_stats.Level);
            }
        }

        /// <summary>
        /// Tire avec l'arme
        /// </summary>
        private void Shoot()
        {
            if (_weaponManager != null && _weaponManager.CanShoot)
            {
                GD.Print("🔫 Tir du joueur!");
                
                // Raycast pour détecter les impacts
                PerformRaycast();
                
                // Effets de tir
                _cameraShake?.ShakeFromShoot();
                _weaponManager.Shoot();
            }
        }

        /// <summary>
        /// Effectue un raycast pour détecter les impacts
        /// </summary>
        private void PerformRaycast()
        {
            var spaceState = GetWorld3D().DirectSpaceState;
            var from = _camera.GlobalPosition;
            var to = _camera.GlobalPosition + _camera.GlobalTransform.Basis.Z * 1000;
            var result = spaceState.IntersectRay(from, to, collisionMask: 1u);
            
            if (result.Count > 0)
            {
                var hit = result[0];
                var hitPosition = new Vector3();
                var hitNormal = new Vector3();
                Node collider = null;
                
                // Accès sécurisé aux propriétés du dictionnaire
                if (hit != null)
                {
                    var hitDict = hit.AsGodotDictionary();
                    if (hitDict.TryGetValue("position", out var posVariant))
                        hitPosition = (Vector3)posVariant;
                    if (hitDict.TryGetValue("normal", out var normalVariant))
                        hitNormal = (Vector3)normalVariant;
                    if (hitDict.TryGetValue("collider", out var colliderVariant))
                        collider = (Node)colliderVariant;
                    
                    GD.Print($"💥 Impact sur {collider?.Name} à {hitPosition}");
                    
                    // Effets visuels
                    _impactEffects?.PlayImpact(hitPosition, hitNormal);
                    
                    // Camera shake
                    _cameraShake?.ShakeFromImpact();
                    
                    // Appliquer des dégâts si cible valide
                    if (collider is Enemy enemy)
                    {
                        enemy.TakeDamage(25);
                    }
                }
            }
        }

        /// <summary>
        /// Recharge l'arme
        /// </summary>
        private void Reload()
        {
            if (_weaponManager != null)
            {
                _weaponManager.Reload();
                GD.Print("🔄 Rechargement de l'arme");
            }
        }

        /// <summary>
        /// Applique des dégâts au joueur
        /// </summary>
        public void TakeDamage(float damage)
        {
            if (_stats != null)
            {
                _stats.TakeDamage(damage);
                
                // Effet de dégâts
                _gameUI?.ShowDamageEffect();
                
                // Camera shake
                _cameraShake?.ShakeFromDamage();
                
                GD.Print($"💔 Le joueur a pris {damage} dégâts. Santé: {_stats.CurrentHealth}/{_stats.MaxHealth}");
            }
        }

        /// <summary>
        /// Nettoie les ressources
        /// </summary>
        public override void _ExitTree()
        {
            GD.Print("🧹 Nettoyage du joueur...");
            
            // Nettoyer les références
            _camera = null;
            _gameUI = null;
            _weaponManager = null;
            _cameraShake = null;
            _impactEffects = null;
            _stats = null;
        }
    }
}
