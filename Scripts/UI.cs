using Godot;
using System;

namespace Firebyte
{
    /// <summary>
    /// Interface utilisateur cyberpunk pour Firebyte FPS
    /// </summary>
    public partial class UI : Control
    {
        // Références aux éléments de l'UI
        private ProgressBar _healthBar;
        private ProgressBar _energyBar;
        private ProgressBar _xpBar;
        private Label _healthLabel;
        private Label _energyLabel;
        private Label _xpLabel;
        private Label _levelLabel;
        private Label _ammoLabel;
        private Label _weaponInfoLabel;
        private ColorRect _damageEffect;
        private ColorRect _crosshair;

        // Références aux stats
        private StatsManager _playerStats;

        // Couleurs cyberpunk
        private readonly Color _neonBlue = new Color(0.0f, 0.8f, 1.0f);
        private readonly Color _neonPink = new Color(1.0f, 0.0f, 0.8f);
        private readonly Color _neonGreen = new Color(0.0f, 1.0f, 0.5f);
        private readonly Color _darkBg = new Color(0.05f, 0.05f, 0.1f, 0.8f);

        public override void _Ready()
        {
            GD.Print("🖥️ Initialisation de l'interface cyberpunk...");
            
            // Configurer l'UI
            SetAnchorsAndOffsetsPreset(Control.Preset.FullRect);
            CreateLayout();
            ApplyCyberpunkStyling();
            
            GD.Print("✅ Interface cyberpunk initialisée");
        }

        /// <summary>
        /// Crée la disposition de l'interface
        /// </summary>
        private void CreateLayout()
        {
            // Panneau principal pour les stats
            var statsPanel = new Panel();
            statsPanel.Position = new Vector2(20, 20);
            statsPanel.Size = new Vector2(300, 150);
            AddChild(statsPanel);

            // Barre de santé
            CreateHealthBar(statsPanel);
            
            // Barre d'énergie
            CreateEnergyBar(statsPanel);
            
            // Barre d'XP
            CreateXPBar(statsPanel);

            // Panneau d'informations
            var infoPanel = new Panel();
            infoPanel.Position = new Vector2(20, 180);
            infoPanel.Size = new Vector2(300, 80);
            AddChild(infoPanel);

            // Labels d'information
            CreateInfoLabels(infoPanel);

            // Panneau des munitions
            var ammoPanel = new Panel();
            ammoPanel.Position = new Vector2(-320, 20);
            ammoPanel.Size = new Vector2(300, 60);
            ammoPanel.SetAnchorsAndOffsetsPreset(Control.Preset.TopRight);
            AddChild(ammoPanel);

            // Labels des munitions
            CreateAmmoLabels(ammoPanel);

            // Effet de dégâts
            CreateDamageEffect();

            // Viseur
            CreateCrosshair();
        }

        /// <summary>
        /// Crée la barre de santé
        /// </summary>
        private void CreateHealthBar(Control parent)
        {
            var healthContainer = new HBoxContainer();
            healthContainer.Position = new Vector2(10, 10);
            parent.AddChild(healthContainer);

            _healthLabel = new Label();
            _healthLabel.Text = "❤️";
            _healthLabel.Size = new Vector2(30, 20);
            healthContainer.AddChild(_healthLabel);

            _healthBar = new ProgressBar();
            _healthBar.Size = new Vector2(200, 20);
            _healthBar.MaxValue = 100;
            _healthBar.Value = 100;
            healthContainer.AddChild(_healthBar);

            var healthValueLabel = new Label();
            healthValueLabel.Size = new Vector2(50, 20);
            healthValueLabel.Text = "100/100";
            healthContainer.AddChild(healthValueLabel);
        }

        /// <summary>
        /// Crée la barre d'énergie
        /// </summary>
        private void CreateEnergyBar(Control parent)
        {
            var energyContainer = new HBoxContainer();
            energyContainer.Position = new Vector2(10, 40);
            parent.AddChild(energyContainer);

            _energyLabel = new Label();
            _energyLabel.Text = "⚡";
            _energyLabel.Size = new Vector2(30, 20);
            energyContainer.AddChild(_energyLabel);

            _energyBar = new ProgressBar();
            _energyBar.Size = new Vector2(200, 20);
            _energyBar.MaxValue = 100;
            _energyBar.Value = 100;
            energyContainer.AddChild(_energyBar);

            var energyValueLabel = new Label();
            energyValueLabel.Size = new Vector2(50, 20);
            energyValueLabel.Text = "100/100";
            energyContainer.AddChild(energyValueLabel);
        }

        /// <summary>
        /// Crée la barre d'XP
        /// </summary>
        private void CreateXPBar(Control parent)
        {
            var xpContainer = new HBoxContainer();
            xpContainer.Position = new Vector2(10, 70);
            parent.AddChild(xpContainer);

            var xpIconLabel = new Label();
            xpIconLabel.Text = "⭐";
            xpIconLabel.Size = new Vector2(30, 20);
            xpContainer.AddChild(xpIconLabel);

            _xpBar = new ProgressBar();
            _xpBar.Size = new Vector2(200, 20);
            _xpBar.MaxValue = 100;
            _xpBar.Value = 0;
            xpContainer.AddChild(_xpBar);

            _xpLabel = new Label();
            _xpLabel.Size = new Vector2(80, 20);
            _xpLabel.Text = "XP: 0/100";
            xpContainer.AddChild(_xpLabel);

            _levelLabel = new Label();
            _levelLabel.Size = new Vector2(60, 20);
            _levelLabel.Text = "Niv. 1";
            xpContainer.AddChild(_levelLabel);
        }

        /// <summary>
        /// Crée les labels d'information
        /// </summary>
        private void CreateInfoLabels(Control parent)
        {
            _weaponInfoLabel = new Label();
            _weaponInfoLabel.Position = new Vector2(10, 10);
            _weaponInfoLabel.Size = new Vector2(280, 30);
            _weaponInfoLabel.Text = "🔫 Assault Rifle";
            parent.AddChild(_weaponInfoLabel);
        }

        /// <summary>
        /// Crée les labels de munitions
        /// </summary>
        private void CreateAmmoLabels(Control parent)
        {
            _ammoLabel = new Label();
            _ammoLabel.Position = new Vector2(10, 10);
            _ammoLabel.Size = new Vector2(280, 40);
            _ammoLabel.Text = "30/30 | 90";
            _ammoLabel.HorizontalAlignment = HorizontalAlignment.Right;
            parent.AddChild(_ammoLabel);
        }

        /// <summary>
        /// Crée l'effet de dégâts
        /// </summary>
        private void CreateDamageEffect()
        {
            _damageEffect = new ColorRect();
            _damageEffect.SetAnchorsAndOffsetsPreset(Control.Preset.FullRect);
            _damageEffect.Color = new Color(1, 0, 0, 0);
            _damageEffect.Visible = false;
            AddChild(_damageEffect);
        }

        /// <summary>
        /// Crée le viseur
        /// </summary>
        private void CreateCrosshair()
        {
            _crosshair = new ColorRect();
            _crosshair.Position = new Vector2(-10, -1);
            _crosshair.Size = new Vector2(20, 2);
            _crosshair.Color = _neonBlue;
            _crosshair.SetAnchorsAndOffsetsPreset(Control.Preset.Center);
            AddChild(_crosshair);

            var crosshairV = new ColorRect();
            crosshairV.Position = new Vector2(-1, -10);
            crosshairV.Size = new Vector2(2, 20);
            crosshairV.Color = _neonBlue;
            crosshairV.SetAnchorsAndOffsetsPreset(Control.Preset.Center);
            AddChild(crosshairV);
        }

        /// <summary>
        /// Applique le style cyberpunk à l'interface
        /// </summary>
        private void ApplyCyberpunkStyling()
        {
            // Style pour les panneaux
            var panelStyle = new StyleBoxFlat();
            panelStyle.BgColor = _darkBg;
            panelStyle.BorderColor = _neonBlue;
            panelStyle.BorderWidthLeft = 2;
            panelStyle.BorderWidthRight = 2;
            panelStyle.BorderWidthTop = 2;
            panelStyle.BorderWidthBottom = 2;
            panelStyle.CornerRadiusTopLeft = 5;
            panelStyle.CornerRadiusTopRight = 5;
            panelStyle.CornerRadiusBottomLeft = 5;
            panelStyle.CornerRadiusBottomRight = 5;

            // Appliquer le style aux panneaux
            foreach (Node child in GetChildren())
            {
                if (child is Panel panel)
                {
                    panel.AddThemeStyleboxOverride("panel", panelStyle);
                }
            }

            // Style pour les barres de progression
            var progressBarStyle = new StyleBoxFlat();
            progressBarStyle.BgColor = new Color(0.1f, 0.1f, 0.2f);
            progressBarStyle.BorderColor = _neonGreen;
            progressBarStyle.BorderWidthBottom = 1;

            var progressBarFillStyle = new StyleBoxFlat();
            progressBarFillStyle.BgColor = _neonGreen;

            // Appliquer le style aux barres
            if (_healthBar != null)
            {
                _healthBar.AddThemeStyleboxOverride("background", progressBarStyle);
                _healthBar.AddThemeStyleboxOverride("fill", progressBarFillStyle);
            }

            if (_energyBar != null)
            {
                var energyFillStyle = new StyleBoxFlat();
                energyFillStyle.BgColor = _neonBlue;
                _energyBar.AddThemeStyleboxOverride("background", progressBarStyle);
                _energyBar.AddThemeStyleboxOverride("fill", energyFillStyle);
            }

            if (_xpBar != null)
            {
                var xpFillStyle = new StyleBoxFlat();
                xpFillStyle.BgColor = _neonPink;
                _xpBar.AddThemeStyleboxOverride("background", progressBarStyle);
                _xpBar.AddThemeStyleboxOverride("fill", xpFillStyle);
            }

            // Style pour les labels
            var labelStyle = new LabelSettings();
            labelStyle.FontColor = _neonGreen;
            labelStyle.FontSize = 14;

            // Appliquer le style aux labels
            ApplyLabelStyle(_healthLabel, labelStyle);
            ApplyLabelStyle(_energyLabel, labelStyle);
            ApplyLabelStyle(_xpLabel, labelStyle);
            ApplyLabelStyle(_levelLabel, labelStyle);
            ApplyLabelStyle(_ammoLabel, labelStyle);
            ApplyLabelStyle(_weaponInfoLabel, labelStyle);
        }

        /// <summary>
        /// Applique le style à un label
        /// </summary>
        private void ApplyLabelStyle(Label label, LabelSettings style)
        {
            if (label != null)
            {
                label.LabelSettings = style;
            }
        }

        /// <summary>
        /// Définit les stats du joueur
        /// </summary>
        public void SetPlayerStats(StatsManager stats)
        {
            _playerStats = stats;
            
            // Connecter les signaux
            if (_playerStats != null)
            {
                _playerStats.HealthChanged += OnHealthChanged;
                _playerStats.EnergyChanged += OnEnergyChanged;
                _playerStats.XPChanged += OnXPChanged;
                _playerStats.LevelUp += OnLevelUp;
            }
        }

        /// <summary>
        /// Met à jour la barre de santé
        /// </summary>
        public void UpdateHealth(float current, float max)
        {
            if (_healthBar != null)
            {
                _healthBar.MaxValue = max;
                _healthBar.Value = current;
                
                // Mettre à jour le label
                var healthValueLabel = _healthBar.GetParent().GetChild<Label>(2);
                if (healthValueLabel != null)
                {
                    healthValueLabel.Text = $"{current:F0}/{max:F0}";
                }
                
                // Changer la couleur selon la santé
                var healthPercentage = current / max;
                Color fillColor;
                
                if (healthPercentage > 0.6f)
                    fillColor = Colors.Green;
                else if (healthPercentage > 0.3f)
                    fillColor = Colors.Yellow;
                else
                    fillColor = Colors.Red;
                
                var fillStyle = new StyleBoxFlat();
                fillStyle.BgColor = fillColor;
                _healthBar.AddThemeStyleboxOverride("fill", fillStyle);
            }
        }

        /// <summary>
        /// Met à jour la barre d'énergie
        /// </summary>
        public void UpdateEnergy(float current, float max)
        {
            if (_energyBar != null)
            {
                _energyBar.MaxValue = max;
                _energyBar.Value = current;
                
                // Mettre à jour le label
                var energyValueLabel = _energyBar.GetParent().GetChild<Label>(2);
                if (energyValueLabel != null)
                {
                    energyValueLabel.Text = $"{current:F0}/{max:F0}";
                }
            }
        }

        /// <summary>
        /// Met à jour la barre d'XP
        /// </summary>
        public void UpdateXP(int current, int toNext, int level)
        {
            if (_xpBar != null && _xpLabel != null && _levelLabel != null)
            {
                _xpBar.MaxValue = toNext;
                _xpBar.Value = current;
                
                _xpLabel.Text = $"XP: {current}/{toNext}";
                _levelLabel.Text = $"Niv. {level}";
            }
        }

        /// <summary>
        /// Affiche l'effet de dégâts
        /// </summary>
        public void ShowDamageEffect()
        {
            if (_damageEffect != null)
            {
                _damageEffect.Visible = true;
                _damageEffect.Color = new Color(1, 0, 0, 0.3f);
                
                // Animation de fondu
                var tween = CreateTween();
                tween.TweenProperty(_damageEffect, "color", new Color(1, 0, 0, 0), 0.3f);
                tween.TweenCallback(Callable.From(() => _damageEffect.Visible = false));
            }
        }

        /// <summary>
        /// Met à jour les informations de l'arme
        /// </summary>
        public void UpdateWeaponInfo(int currentAmmo, int maxAmmo, int reserveAmmo)
        {
            if (_ammoLabel != null)
            {
                _ammoLabel.Text = $"{currentAmmo}/{maxAmmo} | {reserveAmmo}";
            }
        }

        // Gestionnaires d'événements
        private void OnHealthChanged(float current, float max)
        {
            UpdateHealth(current, max);
        }

        private void OnEnergyChanged(float current, float max)
        {
            UpdateEnergy(current, max);
        }

        private void OnXPChanged(int current, int toNext, int level)
        {
            UpdateXP(current, toNext, level);
        }

        private void OnLevelUp(int newLevel)
        {
            // Effet spécial pour le niveau supérieur
            if (_levelLabel != null)
            {
                var originalColor = _levelLabel.LabelSettings.FontColor;
                _levelLabel.LabelSettings.FontColor = _neonPink;
                
                var tween = CreateTween();
                tween.TweenProperty(_levelLabel.LabelSettings, "font_color", originalColor, 1.0f);
            }
        }
    }
}
