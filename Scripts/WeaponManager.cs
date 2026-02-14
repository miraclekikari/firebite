using Godot;
using System;

namespace Firebyte
{
    /// <summary>
    /// Gestionnaire d'armes pour le système de tir
    /// </summary>
    public partial class WeaponManager : Node
    {
        // Événements
        [Signal] public delegate void WeaponFiredEventHandler();
        [Signal] public delegate void WeaponReloadedEventHandler();
        [Signal] public delegate void AmmoChangedEventHandler(int currentAmmo, int maxAmmo, int reserveAmmo);

        // Configuration de l'arme
        [Export] public int MaxAmmo { get; set; } = 30;
        [Export] public int ReserveAmmo { get; set; } = 90;
        [Export] public float FireRate { get; set; } = 600.0f; // coups par minute
        [Export] public float ReloadTime { get; set; } = 2.0f;
        [Export] public float BaseDamage { get; set; } = 25.0f;
        [Export] public float Range { get; set; } = 1000.0f;

        // État actuel
        private int _currentAmmo;
        private bool _isReloading = false;
        private bool _canShoot = true;
        private Timer _fireRateTimer;
        private Timer _reloadTimer;

        // Statistiques
        private int _totalShots = 0;
        private int _totalHits = 0;

        public int CurrentAmmo 
        { 
            get => _currentAmmo;
            private set
            {
                _currentAmmo = Mathf.Clamp(value, 0, MaxAmmo);
                EmitSignal(SignalName.AmmoChanged, _currentAmmo, MaxAmmo, ReserveAmmo);
            }
        }

        public bool IsReloading => _isReloading;
        public bool CanShoot => !_isReloading && _canShoot && CurrentAmmo > 0;
        public float Accuracy => _totalShots > 0 ? (float)_totalHits / _totalShots : 0.0f;

        public override void _Ready()
        {
            GD.Print("🔫 Initialisation du WeaponManager...");
            
            // Initialiser les munitions
            CurrentAmmo = MaxAmmo;
            
            // Configurer les timers
            SetupTimers();
            
            GD.Print($"🔫 Arme prête: {CurrentAmmo}/{MaxAmmo} munitions, {ReserveAmmo} en réserve");
            GD.Print("✅ WeaponManager initialisé");
        }

        /// <summary>
        /// Configure les timers pour le tir et le rechargement
        /// </summary>
        private void SetupTimers()
        {
            // Timer pour la cadence de tir
            _fireRateTimer = new Timer();
            _fireRateTimer.WaitTime = 60.0f / FireRate; // Convertir RPM en secondes
            _fireRateTimer.OneShot = true;
            AddChild(_fireRateTimer);

            // Timer pour le rechargement
            _reloadTimer = new Timer();
            _reloadTimer.WaitTime = ReloadTime;
            _reloadTimer.OneShot = true;
            _reloadTimer.Timeout += OnReloadComplete;
            AddChild(_reloadTimer);
        }

        /// <summary>
        /// Effectue un tir
        /// </summary>
        public void Shoot()
        {
            if (!CanShoot)
            {
                GD.Print("🔫 Impossible de tirer - Rechargement en cours ou pas de munitions");
                return;
            }

            // Consommer une munition
            CurrentAmmo--;
            _totalShots++;
            
            GD.Print($"🔫 Tir! Munitions restantes: {CurrentAmmo}/{MaxAmmo}");
            
            // Démarrer le timer de cadence
            _canShoot = false;
            _fireRateTimer.Start();
            
            // Émettre le signal
            EmitSignal(SignalName.WeaponFired);
            
            // Vérifier si on doit recharger automatiquement
            if (CurrentAmmo == 0 && ReserveAmmo > 0)
            {
                GD.Print("🔄 Plus de munitions - Rechargement automatique");
                Reload();
            }
        }

        /// <summary>
        /// Recharge l'arme
        /// </summary>
        public void Reload()
        {
            if (_isReloading || CurrentAmmo == MaxAmmo || ReserveAmmo == 0)
            {
                GD.Print("🔄 Impossible de recharger - Déjà en cours ou munitions pleines/réserve vide");
                return;
            }

            _isReloading = true;
            GD.Print($"🔄 Rechargement en cours... ({ReloadTime}s)");
            
            // Démarrer le timer de rechargement
            _reloadTimer.Start();
        }

        /// <summary>
        /// Appelé lorsque le rechargement est terminé
        /// </summary>
        private void OnReloadComplete()
        {
            // Calculer les munitions à recharger
            var ammoNeeded = MaxAmmo - CurrentAmmo;
            var ammoToReload = Mathf.Min(ammoNeeded, ReserveAmmo);
            
            CurrentAmmo += ammoToReload;
            ReserveAmmo -= ammoToReload;
            
            _isReloading = false;
            
            GD.Print($"✅ Rechargement terminé! {CurrentAmmo}/{MaxAmmo} munitions, {ReserveAmmo} en réserve");
            
            // Émettre le signal
            EmitSignal(SignalName.WeaponReloaded);
        }

        /// <summary>
        /// Ajoute des munitions
        /// </summary>
        public void AddAmmo(int amount)
        {
            if (amount <= 0) return;
            
            ReserveAmmo += amount;
            GD.Print($"📦 +{amount} munitions ajoutées! Réserve: {ReserveAmmo}");
            
            // Émettre le signal pour mettre à jour l'UI
            EmitSignal(SignalName.AmmoChanged, CurrentAmmo, MaxAmmo, ReserveAmmo);
        }

        /// <summary>
        /// Obtient les dégâts actuels de l'arme
        /// </summary>
        public float GetCurrentDamage()
        {
            return BaseDamage;
        }

        /// <summary>
        /// Enregistre un tir réussi
        /// </summary>
        public void RegisterHit()
        {
            _totalHits++;
            GD.Print($"🎯 Touché! Précision: {Accuracy:P1}");
        }

        /// <summary>
        /// Réinitialise les statistiques de l'arme
        /// </summary>
        public void ResetStats()
        {
            _totalShots = 0;
            _totalHits = 0;
            GD.Print("📊 Statistiques de l'arme réinitialisées");
        }

        /// <summary>
        /// Réinitialise complètement l'arme
        /// </summary>
        public void ResetWeapon()
        {
            // Annuler le rechargement en cours
            if (_isReloading)
            {
                _reloadTimer.Stop();
                _isReloading = false;
            }
            
            // Réinitialiser les munitions
            CurrentAmmo = MaxAmmo;
            ReserveAmmo = 90;
            
            // Réinitialiser l'état
            _canShoot = true;
            
            // Réinitialiser les statistiques
            ResetStats();
            
            GD.Print("🔄 Arme réinitialisée complètement");
        }

        /// <summary>
        /// Obtient des informations sur l'arme
        /// </summary>
        public string GetWeaponInfo()
        {
            return $"Arme: {CurrentAmmo}/{MaxAmmo} | Réserve: {ReserveAmmo} | Précision: {Accuracy:P1}";
        }

        /// <summary>
        /// Vérifie si l'arme peut être rechargée
        /// </summary>
        public bool CanReload()
        {
            return !_isReloading && CurrentAmmo < MaxAmmo && ReserveAmmo > 0;
        }

        /// <summary>
        /// Force l'arrêt du rechargement
        /// </summary>
        public void CancelReload()
        {
            if (_isReloading)
            {
                _reloadTimer.Stop();
                _isReloading = false;
                GD.Print("⏹️ Rechargement annulé");
            }
        }

        public override void _ExitTree()
        {
            // Nettoyer les timers
            if (_fireRateTimer != null)
            {
                _fireRateTimer.QueueFree();
            }
            if (_reloadTimer != null)
            {
                _reloadTimer.QueueFree();
            }
        }
    }
}
