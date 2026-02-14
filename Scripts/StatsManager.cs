using Godot;
using System;

namespace Firebyte
{
    /// <summary>
    /// Gestionnaire de statistiques pour la santé, l'énergie et l'XP
    /// </summary>
    public partial class StatsManager : Node
    {
        // Événements pour les changements de stats
        [Signal] public delegate void HealthChangedEventHandler(float currentHealth, float maxHealth);
        [Signal] public delegate void EnergyChangedEventHandler(float currentEnergy, float maxEnergy);
        [Signal] public delegate void XPChangedEventHandler(int currentXP, int xpToNextLevel, int level);
        [Signal] public delegate void PlayerLevelUpEventHandler(int newLevel);
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
                        EmitSignal(SignalName.Death);
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
                EmitSignal(SignalName.XPChanged, CurrentXP, _xpToNextLevel, Level);
            }
        }

        public override void _Ready()
        {
            GD.Print("📊 Initialisation du StatsManager...");
            
            // Initialiser les valeurs
            _currentHealth = MaxHealth;
            _currentEnergy = MaxEnergy;
            CalculateXPToNextLevel();
            
            // Configurer le timer de régénération
            SetupRegenerationTimer();
            
            GD.Print($"❤️ Santé: {CurrentHealth}/{MaxHealth}");
            GD.Print($"⚡ Énergie: {CurrentEnergy}/{MaxEnergy}");
            GD.Print($"⭐ XP: {CurrentXP}/{XPToNextLevel} (Niveau {Level})");
            GD.Print("✅ StatsManager initialisé");
        }

        /// <summary>
        /// Configure le timer de régénération
        /// </summary>
        private void SetupRegenerationTimer()
        {
            _regenerationTimer = new Timer();
            _regenerationTimer.WaitTime = 0.1f; // 10 fois par seconde
            _regenerationTimer.Autostart = true;
            _regenerationTimer.Timeout += OnRegenerationTick;
            AddChild(_regenerationTimer);
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
                CurrentHealth += HealthRegenerationRate * deltaTime;
            }
            
            // Régénération de l'énergie
            if (CurrentEnergy < MaxEnergy)
            {
                CurrentEnergy += EnergyRegenerationRate * deltaTime;
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
            
            var previousHealth = CurrentHealth;
            CurrentHealth += amount;
            var actualHeal = CurrentHealth - previousHealth;
            
            GD.Print($"💚 Soins: {actualHeal:F1}. Santé actuelle: {CurrentHealth}/{MaxHealth}");
            OnHealed(actualHeal);
        }

        /// <summary>
        /// Utilise de l'énergie
        /// </summary>
        public bool UseEnergy(float amount)
        {
            if (amount <= 0) return true;
            
            if (CurrentEnergy >= amount)
            {
                CurrentEnergy -= amount;
                GD.Print($"⚡ Énergie utilisée: {amount:F1}. Énergie actuelle: {CurrentEnergy}/{MaxEnergy}");
                OnEnergyUsed(amount);
                return true;
            }
            
            GD.Print($"❌ Énergie insuffisante: besoin {amount}, disponible {CurrentEnergy}");
            return false;
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
            
            GD.Print($"🎉 NIVEAU SUPÉRIEUR! Niveau {Level}");
            GD.Print($"💊 Nouvelle santé max: {MaxHealth}");
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
            CurrentHealth = MaxHealth;
            CurrentEnergy = MaxEnergy;
            CurrentXP = 0;
            Level = 1;
            CalculateXPToNextLevel();
            
            GD.Print("🔄 Stats réinitialisées");
        }

        /// <summary>
        /// Obtient le pourcentage de santé
        /// </summary>
        public float GetHealthPercentage()
        {
            return CurrentHealth / MaxHealth;
        }

        /// <summary>
        /// Obtient le pourcentage d'énergie
        /// </summary>
        public float GetEnergyPercentage()
        {
            return CurrentEnergy / MaxEnergy;
        }

        /// <summary>
        /// Obtient le pourcentage d'XP pour le niveau actuel
        /// </summary>
        public float GetXPPercentage()
        {
            return (float)CurrentXP / XPToNextLevel;
        }

        /// <summary>
        /// Vérifie si le personnage est en vie
        /// </summary>
        public bool IsAlive()
        {
            return CurrentHealth > 0;
        }

        /// <summary>
        /// Vérifie si le personnage a suffisamment d'énergie
        /// </summary>
        public bool HasEnoughEnergy(float amount)
        {
            return CurrentEnergy >= amount;
        }

        // Méthodes virtuelles pour les effets (peuvent être surchargées)
        protected virtual void OnDamageTaken(float damage)
        {
            // Effet visuel/sonore de dégâts
        }

        protected virtual void OnHealed(float amount)
        {
            // Effet visuel/sonore de soin
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

        public override void _ExitTree()
        {
            // Nettoyer le timer
            if (_regenerationTimer != null)
            {
                _regenerationTimer.QueueFree();
            }
        }
    }
}
