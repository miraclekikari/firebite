using Godot;

namespace Firebyte
{
    /// <summary>
    /// Contrôleur de joueur FPS avec mouvements fluides et système de tir Raycast
    /// </summary>
    public partial class Player : CharacterBody3D
    {
        // Références aux composants
        private Camera3D _camera;
        private CollisionShape3D _collisionShape;
        private StatsManager _stats;
        private WeaponManager _weaponManager;
        private UI _gameUI;
        private CameraShake _cameraShake;
        private ImpactEffects _impactEffects;

        // Paramètres de mouvement
        [Export] public float Speed { get; set; } = 5.0f;
        [Export] public float SprintSpeed { get; set; } = 8.0f;
        [Export] public float JumpVelocity { get; set; } = 4.5f;
        [Export] public float MouseSensitivity { get; set; } = 0.002f;
        [Export] public float Acceleration { get; set; } = 20.0f;
        [Export] public float Friction { get; set; } = 10.0f;

        // État du joueur
        private Vector3 _velocity = Vector3.Zero;
        private Vector2 _lookDirection = Vector2.Zero;
        private bool _isSprinting = false;
        private bool _isGrounded = false;

        // Gravité
        private float _gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

        public override void _Ready()
        {
            GD.Print("🎮 Initialisation du Player...");
            
            // Initialiser les composants
            InitializeComponents();
            InitializeCamera();
            InitializeStats();
            InitializeWeapons();
            InitializeUI();
            InitializeEffects();
            
            // Capturer le curseur
            Input.SetMouseMode(Input.MouseModeEnum.Captured);
            
            GD.Print("✅ Player initialisé avec succès");
        }

        /// <summary>
        /// Initialise les composants de base du joueur
        /// </summary>
        private void InitializeComponents()
        {
            // Créer la collision shape
            _collisionShape = new CapsuleShape3D();
            _collisionShape.Height = 1.8f;
            _collisionShape.Radius = 0.4f;
            
            var collisionNode = new CollisionShape3D();
            collisionNode.Shape = _collisionShape;
            collisionNode.Position = new Vector3(0, 0, 0.9f);
            AddChild(collisionNode);
        }

        /// <summary>
        /// Initialise la caméra FPS
        /// </summary>
        private void InitializeCamera()
        {
            _camera = new Camera3D();
            _camera.Name = "Camera3D";
            _camera.Position = new Vector3(0, 0, 0.6f);
            _camera.Fov = 75.0f;
            _camera.Near = 0.1f;
            _camera.Far = 1000.0f;
            
            AddChild(_camera);
        }

        /// <summary>
        /// Initialise le gestionnaire de statistiques
        /// </summary>
        private void InitializeStats()
        {
            _stats = new StatsManager();
            _stats.Name = "StatsManager";
            AddChild(_stats);
            
            GD.Print($"❤️ Santé: {_stats.CurrentHealth}/{_stats.MaxHealth}");
            GD.Print($"⚡ Énergie: {_stats.CurrentEnergy}/{_stats.MaxEnergy}");
            GD.Print($"⭐ XP: {_stats.CurrentXP}/{_stats.XPToNextLevel} (Niveau {_stats.Level})");
        }

        /// <summary>
        /// Initialise le gestionnaire d'armes
        /// </summary>
        private void InitializeWeapons()
        {
            _weaponManager = new WeaponManager();
            _weaponManager.Name = "WeaponManager";
            AddChild(_weaponManager);
        }

        /// <summary>
        /// Initialise l'interface utilisateur
        /// </summary>
        private void InitializeUI()
        {
            _gameUI = GetNode<UI>("../GameUI");
            if (_gameUI != null)
            {
                _gameUI.SetPlayerStats(_stats);
                GD.Print("🖥️ Interface connectée au joueur");
            }
        }

        public override void _PhysicsProcess(double delta)
        {
            var deltaTime = (float)delta;
            
            // Gérer la gravité
            if (!IsOnFloor())
                _velocity.Y -= _gravity * deltaTime;
            else
                _isGrounded = true;

            // Gérer le mouvement
            HandleMovement(deltaTime);
            
            // Appliquer le mouvement
            Velocity = _velocity;
            MoveAndSlide();
            
            // Mettre à jour l'UI
            UpdateUI();
        }

        /// <summary>
        /// Gère les mouvements du joueur
        /// </summary>
        private void HandleMovement(float deltaTime)
        {
            // Calculer la direction de mouvement
            var inputDir = Input.GetVector("move_left", "move_right", "move_forward", "move_backward");
            var direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
            
            // Vitesse actuelle
            var currentSpeed = _isSprinting ? SprintSpeed : Speed;
            
            // Appliquer l'accélération
            if (direction.Length() > 0)
            {
                _velocity.X = Mathf.MoveToward(_velocity.X, direction.X * currentSpeed, Acceleration * deltaTime);
                _velocity.Z = Mathf.MoveToward(_velocity.Z, direction.Z * currentSpeed, Acceleration * deltaTime);
            }
            else
            {
                // Appliquer le frottement
                _velocity.X = Mathf.MoveToward(_velocity.X, 0, Friction * deltaTime);
                _velocity.Z = Mathf.MoveToward(_velocity.Z, 0, Friction * deltaTime);
            }
        }

        public override void _Input(InputEvent @event)
        {
            // Gérer le saut
            if (@event is InputEventKey jumpEvent && jumpEvent.Pressed && jumpEvent.Keycode == Key.Space)
            {
                if (IsOnFloor())
                {
                    _velocity.Y = JumpVelocity;
                    GD.Print("🦘 Saut!");
                }
            }
            
            // Gérer le sprint
            if (@event is InputEventKey sprintEvent)
            {
                if (sprintEvent.Pressed && sprintEvent.Keycode == Key.Shift)
                {
                    _isSprinting = true;
                    GD.Print("🏃 Sprint activé");
                }
                else if (!sprintEvent.Pressed && sprintEvent.Keycode == Key.Shift)
                {
                    _isSprinting = false;
                    GD.Print("🚶 Sprint désactivé");
                }
            }
            
            // Gérer le tir
            if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
            {
                Shoot();
            }
            
            // Gérer le rechargement
            if (@event is InputEventKey reloadEvent && reloadEvent.Pressed && reloadEvent.Keycode == Key.R)
            {
                _weaponManager?.Reload();
            }
            
            // Gérer le mouvement de la souris
            if (@event is InputEventMouseMotion mouseMotion)
            {
                HandleMouseLook(mouseMotion);
            }
        }

        /// <summary>
        /// Gère le mouvement de la caméra (look)
        /// </summary>
        private void HandleMouseLook(InputEventMouseMotion mouseMotion)
        {
            // Rotation horizontale (tourner le corps)
            RotateY(-mouseMotion.Relative.X * MouseSensitivity);
            
            // Rotation verticale (incliner la caméra)
            _camera.RotateX(-mouseMotion.Relative.Y * MouseSensitivity);
            
            // Limiter l'angle vertical
            var currentRotation = _camera.RotationDegrees;
            currentRotation.X = Mathf.Clamp(currentRotation.X, -89, 89);
            _camera.RotationDegrees = currentRotation;
        }

        /// <summary>
        /// Effectue un tir avec raycast ultra-rapide
        /// </summary>
        private void Shoot()
        {
            if (_weaponManager == null || !_weaponManager.CanShoot())
            {
                GD.Print("🔫 Impossible de tirer - Rechargement ou munitions insuffisantes");
                return;
            }

            GD.Print("🔫 Tir!");
            
            // Camera Shake pour le tir
            _cameraShake?.ShakeFromShoot();
            
            // Créer le raycast depuis la caméra
            var spaceState = GetWorld3D().DirectSpaceState;
            var from = _camera.GlobalPosition;
            var to = from + -_camera.GlobalTransform.Basis.Z * 1000; // 1000 unités de portée
            
            var query = PhysicsRayQueryParameters3D.Create(from, to);
            query.CollisionMask = 1; // Layer 1 pour les objets touchables
            
            var result = spaceState.IntersectRay(query);
            
            if (result.Count > 0)
            {
                var hitPosition = (Vector3)result["position"];
                var hitNormal = (Vector3)result["normal"];
                var hitObject = (GodotObject)result["collider"];
                
                GD.Print($"✅ Touché! Position: {hitPosition}, Normal: {hitNormal}");
                GD.Print($"🎯 Objet touché: {hitObject.GetType().Name}");
                
                // Appliquer les dégâts si l'objet a un StatsManager
                var hitNode = (Node)hitObject;
                var hitStats = hitNode.GetNode<StatsManager>("StatsManager");
                if (hitStats != null)
                {
                    var damage = _weaponManager.GetCurrentDamage();
                    hitStats.TakeDamage(damage);
                    GD.Print($"💥 {damage} dégâts infligés!");
                    
                    // Camera Shake pour l'impact
                    _cameraShake?.ShakeFromImpact(damage);
                    
                    // Effets d'impact
                    _impactEffects?.CreateImpact(hitPosition, hitNormal, "metal");
                    
                    // Enregistrer le tir réussi
                    _weaponManager.RegisterHit();
                }
                else
                {
                    // Effet d'impact même sans StatsManager
                    _impactEffects?.CreateImpact(hitPosition, hitNormal, "metal");
                }
                
                // Créer un effet visuel
                CreateHitEffect(hitPosition, hitNormal);
            }
            else
            {
                GD.Print("❌ Raté - Aucune cible touchée");
            }
            
            _weaponManager.Shoot();
        }

        /// <summary>
        /// Crée un effet visuel au point d'impact
        /// </summary>
        private void CreateHitEffect(Vector3 position, Vector3 normal)
        {
            // Créer une sphère temporaire pour l'impact
            var impact = new MeshInstance3D();
            impact.Mesh = new SphereMesh();
            impact.Mesh.Radius = 0.1f;
            impact.Position = position;
            
            var material = new StandardMaterial3D();
            material.AlbedoColor = Colors.Yellow;
            material.EmissionEnabled = true;
            material.Emission = Colors.Yellow;
            impact.MaterialOverride = material;
            
            GetTree().CurrentScene.AddChild(impact);
            
            // Supprimer après 0.5 secondes
            var timer = GetTree().CreateTimer(0.5);
            timer.Timeout += () => impact.QueueFree();
        }

        /// <summary>
        /// Met à jour l'interface utilisateur
        /// </summary>
        private void UpdateUI()
        {
            if (_gameUI != null && _stats != null)
            {
                _gameUI.UpdateHealth(_stats.CurrentHealth, _stats.MaxHealth);
                _gameUI.UpdateEnergy(_stats.CurrentEnergy, _stats.MaxEnergy);
                _gameUI.UpdateXP(_stats.CurrentXP, _stats.XPToNextLevel, _stats.Level);
            }
        }

        /// Crée un effet visuel de dégâts
        /// </summary>
        private void CreateDamageEffect()
        {
            // Flash rouge sur l'écran
            if (_gameUI != null)
            {
                _gameUI.ShowDamageEffect();
            }
        }

        /// <summary>
        /// Soigne le joueur
        /// </summary>
        public void Heal(float amount)
        {
            _stats?.Heal(amount);
            GD.Print($"💚 Le joueur est soigné de {amount} points! Santé: {_stats.CurrentHealth}/{_stats.MaxHealth}");
        }

        /// <summary>
        /// Ajoute de l'XP au joueur
        /// </summary>
        public void AddXP(int amount)
        {
            _stats?.AddXP(amount);
            GD.Print($"⭐ +{amount} XP gagnés! Niveau: {_stats.Level}");
        }

        /// <summary>
        /// Initialise les effets visuels et sonores
        /// </summary>
        private void InitializeEffects()
        {
            GD.Print("⚡ Initialisation des effets...");
            
            // Camera Shake
            _cameraShake = new CameraShake();
            _cameraShake.Name = "CameraShake";
            AddChild(_cameraShake);
            
            // Configurer le camera shake avec la caméra
            if (_camera != null)
            {
                _cameraShake.SetupCamera(_camera);
            }
            
            // Impact Effects
            _impactEffects = new ImpactEffects();
            _impactEffects.Name = "ImpactEffects";
            AddChild(_impactEffects);
            
            GD.Print("✅ Effets initialisés");
        }

        /// <summary>
        /// Obtient la position de la caméra pour le raycast
        /// </summary>
        public Vector3 GetCameraPosition()
        {
            return _camera?.GlobalPosition ?? GlobalPosition;
        }

        /// <summary>
        /// Obtient la direction de la caméra
        /// </summary>
        public Vector3 GetCameraDirection()
        {
            return _camera != null ? -_camera.GlobalTransform.Basis.Z : -Transform.Basis.Z;
        }
    }
}
