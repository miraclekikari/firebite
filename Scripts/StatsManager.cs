using Godot;
using System;

namespace Firebyte
{
    /// <summary>
    /// Gestionnaire de statistiques simplifié compatible Godot 4.2
    /// </summary>
    public partial class StatsManager : Node
    {
        // Événements
        [Signal] public delegate void HealthChangedEventHandler(float current, float max);
        [Signal] public delegate void EnergyChangedEventHandler(float current, float max);
        [Signal] public delegate void XPChangedEventHandler(int current, int max);
        [Signal] public delegate void PlayerLevelUpEventHandler(int level);
        [Signal] public delegate void DeathEventHandler();

        // Propriétés de santé
        [Export] public float MaxHealth { get; set; } = 100.0f;
        [Export] public float HealthRegenerationRate { get; set; } = 2.0f; // par seconde

        // Propriétés d'énergie
        [Export] public float MaxEnergy { get; set; } = 100.0f;
        [Export] public float EnergyRegenerationRate { get; set; } = 5.0f; // par seconde

        // Propriétés d'XP et niveau
        [Export] public int CurrentXP { get; private set; } = 0;
        [Export] public int Level { get; private set; } = 1;
        [Export] public float XPMultiplier { get; set; } = 1.0f;

        // Variables internes
        private float _currentHealth;
        private float _currentEnergy;
        private int _xpToNextLevel;

        // Timer pour la régénération
        private Timer _regenerationTimer;

        public float CurrentHealth 
        { 
            get => _currentHealth;
            private set
            {
                if (Math.Abs(_currentHealth - value) > 0.01f)
                {
                    _currentHealth = Mathf.Clamp(value, 0, MaxHealth);
                    EmitSignal(SignalName.HealthChanged, _currentHealth, MaxHealth);
                    
                    if (_currentHealth <= 0)
                    {
                        OnDeath();
                    }
                }
            }
        }

        public float CurrentEnergy 
        { 
            get => _currentEnergy;
            private set
            {
                if (Math.Abs(_currentEnergy - value) > 0.01f)
                {
                    _currentEnergy = Mathf.Clamp(value, 0, MaxEnergy);
                    EmitSignal(SignalName.EnergyChanged, _currentEnergy, MaxEnergy);
                }
            }
        }

        public int XPToNextLevel 
        { 
            get => _xpToNextLevel;
            private set
            {
                _xpToNextLevel = value;
                EmitSignal(SignalName.XPChanged, CurrentXP, _xpToNextLevel);
            }
        }

        public override void _Ready()
        {
            GD.Print("📊 Initialisation du gestionnaire de statistiques...");
            
            // Initialiser les valeurs
            _currentHealth = MaxHealth;
            _currentEnergy = MaxEnergy;
            CalculateXPToNextLevel();
            
            // Configurer le timer de régénération
            SetupRegenerationTimer();
            
            GD.Print("✅ Gestionnaire de statistiques initialisé");
        }

        /// <summary>
        /// Configure le timer de régénération
        /// </summary>
        private void SetupRegenerationTimer()
        {
            _regenerationTimer = new Timer();
            _regenerationTimer.WaitTime = 0.5f; // 2 fois par seconde
            _regenerationTimer.Timeout += OnRegenerationTick;
            AddChild(_regenerationTimer);
            _regenerationTimer.Start();
        }

        /// <summary>
        /// Gère la régénération automatique
        /// </summary>
        private void OnRegenerationTick()
        {
            var deltaTime = _regenerationTimer.WaitTime;
            
            // Régénération de la santé
            if (CurrentHealth < MaxHealth)
            {
                CurrentHealth += (float)(HealthRegenerationRate * deltaTime);
            }
            
            // Régénération de l'énergie
            if (CurrentEnergy < MaxEnergy)
            {
                CurrentEnergy += (float)(EnergyRegenerationRate * deltaTime);
            }
        }

        /// <summary>
        /// Applique des dégâts
        /// </summary>
        public void TakeDamage(float damage)
        {
            if (damage <= 0) return;
            
            CurrentHealth -= damage;
            GD.Print($"💥 Dégâts reçus: {damage}. Santé actuelle: {CurrentHealth}/{MaxHealth}");
            
            // Effet de feedback visuel (peut être étendu)
            OnDamageTaken(damage);
        }

        /// <summary>
        /// Soigne le personnage
        /// </summary>
        public void Heal(float amount)
        {
            if (amount <= 0) return;
            
            CurrentHealth += amount;
            GD.Print($"💚 Soins reçus: {amount}. Santé actuelle: {CurrentHealth}/{MaxHealth}");
            
            OnHealed(amount);
        }

        /// <summary>
        /// Utilise de l'énergie
        /// </summary>
        public void UseEnergy(float amount)
        {
            if (amount <= 0) return;
            
            CurrentEnergy -= amount;
            GD.Print($"⚡ Énergie utilisée: {amount}. Énergie actuelle: {CurrentEnergy}/{MaxEnergy}");
            
            OnEnergyUsed(amount);
        }

        /// <summary>
        /// Ajoute de l'XP
        /// </summary>
        public void AddXP(int amount)
        {
            if (amount <= 0) return;
            
            var actualAmount = (int)(amount * XPMultiplier);
            CurrentXP += actualAmount;
            GD.Print($"⭐ XP gagnée: {actualAmount}. Total: {CurrentXP}/{XPToNextLevel}");
            
            // Vérifier si on peut monter en niveau
            CheckLevelUp();
            
            OnXPGained(actualAmount);
        }

        /// <summary>
        /// Vérifie et gère le passage au niveau supérieur
        /// </summary>
        private void CheckLevelUp()
        {
            while (CurrentXP >= XPToNextLevel)
            {
                LevelUp();
            }
        }

        /// <summary>
        /// Fait monter le personnage d'un niveau
        /// </summary>
        private void LevelUp()
        {
            Level++;
            CurrentXP -= XPToNextLevel;
            CalculateXPToNextLevel();
            
            // Augmenter les stats de base
            MaxHealth += 10;
            MaxEnergy += 5;
            HealthRegenerationRate += 0.2f;
            EnergyRegenerationRate += 0.3f;
            
            // Restaurer la santé et l'énergie au maximum
            CurrentHealth = MaxHealth;
            CurrentEnergy = MaxEnergy;
            
            GD.Print($"🎉 Niveau supérieur! Nouveau niveau: {Level}");
            GD.Print($"❤️ Nouvelle santé max: {MaxHealth}");
            GD.Print($"⚡ Nouvelle énergie max: {MaxEnergy}");
            
            EmitSignal(SignalName.PlayerLevelUp, Level);
            OnLevelUp();
        }

        /// <summary>
        /// Calcule l'XP nécessaire pour le prochain niveau
        /// </summary>
        private void CalculateXPToNextLevel()
        {
            XPToNextLevel = (int)(100 * Mathf.Pow(1.5f, Level - 1));
        }

        /// <summary>
        /// Réinitialise toutes les stats
        /// </summary>
        public void ResetStats()
        {
            GD.Print("🔄 Réinitialisation des statistiques...");
            
            Level = 1;
            CurrentXP = 0;
            MaxHealth = 100;
            MaxEnergy = 100;
            HealthRegenerationRate = 2.0f;
            EnergyRegenerationRate = 5.0f;
            XPMultiplier = 1.0f;
            
            CurrentHealth = MaxHealth;
            CurrentEnergy = MaxEnergy;
            CalculateXPToNextLevel();
            
            GD.Print("✅ Statistiques réinitialisées");
        }

        // Méthodes virtuelles pour les effets visuels/sonores
        protected virtual void OnDamageTaken(float damage)
        {
            // Effet visuel/sonore de dégâts
        }

        protected virtual void OnHealed(float amount)
        {
            // Effet visuel/sonore de soins
        }

        protected virtual void OnEnergyUsed(float amount)
        {
            // Effet visuel/sonore d'utilisation d'énergie
        }

        protected virtual void OnXPGained(int amount)
        {
            // Effet visuel/sonore de gain d'XP
        }

        protected virtual void OnLevelUp()
        {
            // Effet visuel/sonore de niveau supérieur
        }

        protected virtual void OnDeath()
        {
            // Effet visuel/sonore de mort
            EmitSignal(SignalName.Death);
        }

        /// <summary>
        /// Nettoie les ressources
        /// </summary>
        public override void _ExitTree()
        {
            GD.Print("🧹 Nettoyage du gestionnaire de statistiques...");
            
            // Nettoyer le timer
            if (_regenerationTimer != null)
            {
                _regenerationTimer.QueueFree();
            }
        }
    }
}
