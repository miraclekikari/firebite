using Godot;
using System;

namespace Firebyte
{
    /// <summary>
    /// Gestionnaire d'armes simplifié compatible Godot 4.2
    /// </summary>
    public partial class WeaponManager : Node
    {
        // Références aux composants
        private AudioStreamPlayer3D _shootSound;
        private AudioStreamPlayer3D _reloadSound;
        private Timer _fireRateTimer;
        private Timer _reloadTimer;

        // Paramètres de l'arme
        [Export] public int MaxAmmo { get; set; } = 30;
        [Export] public float FireRate { get; set; } = 600; // coups par minute
        [Export] public float ReloadTime { get; set; } = 2.0f; // secondes
        [Export] public float Damage { get; set; } = 25.0f;

        // État de l'arme
        private int _currentAmmo;
        private bool _isReloading;
        private bool _canShoot = true;

        // Événements
        [Signal] public delegate void AmmoChangedEventHandler(int current, int max);
        [Signal] public delegate void ReloadStartedEventHandler();
        [Signal] public delegate void ReloadFinishedEventHandler();

        public override void _Ready()
        {
            GD.Print("🔫 Initialisation du gestionnaire d'armes...");
            
            InitializeComponents();
            SetupTimers();
            
            GD.Print("✅ Gestionnaire d'armes initialisé");
        }

        /// <summary>
        /// Initialise les composants
        /// </summary>
        private void InitializeComponents()
        {
            // Créer les sons
            _shootSound = new AudioStreamPlayer3D();
            _shootSound.Name = "ShootSound";
            AddChild(_shootSound);

            _reloadSound = new AudioStreamPlayer3D();
            _reloadSound.Name = "ReloadSound";
            AddChild(_reloadSound);
        }

        /// <summary>
        /// Configure les timers
        /// </summary>
        private void SetupTimers()
        {
            // Timer de cadence de tir
            _fireRateTimer = new Timer();
            _fireRateTimer.WaitTime = 60.0f / FireRate;
            _fireRateTimer.Timeout += OnFireRateTick;
            AddChild(_fireRateTimer);
            _fireRateTimer.Start();

            // Timer de rechargement
            _reloadTimer = new Timer();
            _reloadTimer.WaitTime = ReloadTime;
            _reloadTimer.Timeout += OnReloadFinished;
            AddChild(_reloadTimer);
        }

        /// <summary>
        /// Gère la cadence de tir
        /// </summary>
        private void OnFireRateTick()
        {
            _canShoot = true;
        }

        /// <summary>
        /// Gère la fin du rechargement
        /// </summary>
        private void OnReloadFinished()
        {
            _isReloading = false;
            _canShoot = true;
            
            EmitSignal(SignalName.ReloadFinished);
            GD.Print("🔄 Rechargement terminé");
        }

        /// <summary>
        /// Tire avec l'arme
        /// </summary>
        public void Shoot()
        {
            if (!_canShoot || _currentAmmo <= 0)
            {
                GD.Print("🔫 Impossible de tirer - Pas de munitions");
                return;
            }

            GD.Print("🔫 Tir!");
            
            // Consommer une munition
            _currentAmmo--;
            
            // Jouer le son de tir
            if (_shootSound != null)
            {
                _shootSound.Play();
            }

            // Émettre le signal de changement de munitions
            EmitSignal(SignalName.AmmoChanged, _currentAmmo, MaxAmmo);
            
            // Démarrer le timer de cadence
            _fireRateTimer.Start();
            
            GD.Print($"🔫 Munitions restantes: {_currentAmmo}/{MaxAmmo}");
        }

        /// <summary>
        /// Recharge l'arme
        /// </summary>
        public void Reload()
        {
            if (_isReloading)
            {
                GD.Print("🔄 Rechargement déjà en cours...");
                return;
            }

            GD.Print("🔄 Début du rechargement...");
            
            _isReloading = true;
            _canShoot = false;
            
            // Jouer le son de rechargement
            if (_reloadSound != null)
            {
                _reloadSound.Play();
            }

            // Émettre le signal de début de rechargement
            EmitSignal(SignalName.ReloadStarted);
            
            // Démarrer le timer de reloading
            _reloadTimer.Start();
            
            GD.Print("🔄 Rechargement en cours...");
        }

        /// <summary>
        /// Indique si l'arme peut tirer
        /// </summary>
        public bool CanShoot
        {
            get { return _canShoot && _currentAmmo > 0; }
        }

        /// <summary>
        /// Obtient le nombre de munitions actuelles
        /// </summary>
        public int GetCurrentAmmo()
        {
            return _currentAmmo;
        }

        /// <summary>
        /// Ajoute des munitions
        /// </summary>
        public void AddAmmo(int amount)
        {
            if (amount > 0)
            {
                _currentAmmo = Mathf.Min(_currentAmmo + amount, MaxAmmo);
                GD.Print($"🔫 +{amount} munitions ajoutées. Total: {_currentAmmo}/{MaxAmmo}");
                
                // Émettre le signal de changement
                EmitSignal(SignalName.AmmoChanged, _currentAmmo, MaxAmmo);
            }
        }

        /// <summary>
        /// Nettoie les ressources
        /// </summary>
        public override void _ExitTree()
        {
            GD.Print("🧹 Nettoyage du gestionnaire d'armes...");
            
            // Nettoyer les références
            _shootSound = null;
            _reloadSound = null;
            _fireRateTimer = null;
            _reloadTimer = null;
        }
    }
}
