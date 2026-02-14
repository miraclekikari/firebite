using Godot;
using System;

namespace Firebyte
{
    /// <summary>
    /// Interface utilisateur simplifiée compatible Godot 4.2
    /// </summary>
    public partial class UI : Control
    {
        // Références aux éléments de l'interface
        private ProgressBar _healthBar;
        private ProgressBar _energyBar;
        private ProgressBar _xpBar;
        private Label _healthLabel;
        private Label _energyLabel;
        private Label _xpLabel;
        private Label _levelLabel;
        private Label _ammoLabel;
        private ColorRect _damageEffect;
        private ColorRect _crosshair;
        private ColorRect _crosshairV;

        // Couleurs cyberpunk
        private readonly Color _neonBlue = new Color(0.0f, 0.8f, 1.0f);
        private readonly Color _neonPink = new Color(1.0f, 0.0f, 0.8f);
        private readonly Color _neonGreen = new Color(0.0f, 1.0f, 0.5f);
        private readonly Color _darkBg = new Color(0.05f, 0.05f, 0.1f, 0.8f);

        public override void _Ready()
        {
            GD.Print("🖥️ Initialisation de l'interface cyberpunk...");
            
            // Configurer l'UI
            SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
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
            ammoPanel.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.TopRight);
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
            _healthBar = new ProgressBar();
            _healthBar.Position = new Vector2(10, 10);
            _healthBar.Size = new Vector2(280, 20);
            _healthBar.MaxValue = 100;
            _healthBar.Value = 100;
            _healthBar.Modulate = _neonGreen;
            parent.AddChild(_healthBar);

            // Label de santé
            _healthLabel = new Label();
            _healthLabel.Position = new Vector2(10, 35);
            _healthLabel.Text = "Santé: 100/100";
            _healthLabel.Modulate = _neonGreen;
            parent.AddChild(_healthLabel);
        }

        /// <summary>
        /// Crée la barre d'énergie
        /// </summary>
        private void CreateEnergyBar(Control parent)
        {
            _energyBar = new ProgressBar();
            _energyBar.Position = new Vector2(10, 60);
            _energyBar.Size = new Vector2(280, 20);
            _energyBar.MaxValue = 100;
            _energyBar.Value = 100;
            _energyBar.Modulate = _neonBlue;
            parent.AddChild(_energyBar);

            // Label d'énergie
            _energyLabel = new Label();
            _energyLabel.Position = new Vector2(10, 85);
            _energyLabel.Text = "Énergie: 100/100";
            _energyLabel.Modulate = _neonBlue;
            parent.AddChild(_energyLabel);
        }

        /// <summary>
        /// Crée la barre d'XP
        /// </summary>
        private void CreateXPBar(Control parent)
        {
            _xpBar = new ProgressBar();
            _xpBar.Position = new Vector2(10, 110);
            _xpBar.Size = new Vector2(280, 20);
            _xpBar.MaxValue = 100;
            _xpBar.Value = 0;
            _xpBar.Modulate = _neonPink;
            parent.AddChild(_xpBar);

            // Label de niveau
            _levelLabel = new Label();
            _levelLabel.Position = new Vector2(10, 135);
            _levelLabel.Text = "Niveau: 1";
            _levelLabel.Modulate = _neonPink;
            parent.AddChild(_levelLabel);
        }

        /// <summary>
        /// Crée les labels d'information
        /// </summary>
        private void CreateInfoLabels(Control parent)
        {
            _xpLabel = new Label();
            _xpLabel.Position = new Vector2(10, 10);
            _xpLabel.Text = "XP: 0/100";
            _xpLabel.Modulate = _neonPink;
            parent.AddChild(_xpLabel);
        }

        /// <summary>
        /// Crée les labels de munitions
        /// </summary>
        private void CreateAmmoLabels(Control parent)
        {
            _ammoLabel = new Label();
            _ammoLabel.Position = new Vector2(10, 10);
            _ammoLabel.Text = "Munitions: 30/30";
            _ammoLabel.Modulate = _neonBlue;
            parent.AddChild(_ammoLabel);
        }

        /// <summary>
        /// Crée l'effet de dégâts
        /// </summary>
        private void CreateDamageEffect()
        {
            _damageEffect = new ColorRect();
            _damageEffect.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
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
            _crosshair.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.Center);
            AddChild(_crosshair);

            _crosshairV = new ColorRect();
            _crosshairV.Position = new Vector2(-1, -10);
            _crosshairV.Size = new Vector2(2, 20);
            _crosshairV.Color = _neonBlue;
            _crosshairV.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.Center);
            AddChild(_crosshairV);
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

            // Appliquer le style aux panneaux
            foreach (Panel panel in GetChildren())
            {
                if (panel is Panel p)
                {
                    p.AddThemeStyleboxOverride("panel", panelStyle);
                }
            }

            // Style pour les barres de progression
            var progressStyle = new StyleBoxFlat();
            progressStyle.BgColor = new Color(0.1f, 0.1f, 0.1f, 0.8f);
            progressStyle.BorderColor = _neonBlue;
            progressStyle.BorderWidthLeft = 1;
            progressStyle.BorderWidthRight = 1;
            progressStyle.BorderWidthTop = 1;
            progressStyle.BorderWidthBottom = 1;

            // Appliquer le style aux barres
            if (_healthBar != null)
            {
                _healthBar.AddThemeStyleboxOverride("fill", progressStyle);
            }
            if (_energyBar != null)
            {
                _energyBar.AddThemeStyleboxOverride("fill", progressStyle);
            }
            if (_xpBar != null)
            {
                _xpBar.AddThemeStyleboxOverride("fill", progressStyle);
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
            }
            if (_healthLabel != null)
            {
                _healthLabel.Text = $"Santé: {current:F0}/{max:F0}";
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
            }
            if (_energyLabel != null)
            {
                _energyLabel.Text = $"Énergie: {current:F0}/{max:F0}";
            }
        }

        /// <summary>
        /// Met à jour la barre d'XP
        /// </summary>
        public void UpdateXP(int current, int max)
        {
            if (_xpBar != null)
            {
                _xpBar.MaxValue = max;
                _xpBar.Value = current;
            }
            if (_xpLabel != null)
            {
                _xpLabel.Text = $"XP: {current}/{max}";
            }
        }

        /// <summary>
        /// Met à jour le niveau
        /// </summary>
        public void UpdateLevel(int level)
        {
            if (_levelLabel != null)
            {
                _levelLabel.Text = $"Niveau: {level}";
            }
        }

        /// <summary>
        /// Met à jour les munitions
        /// </summary>
        public void UpdateAmmo(int current, int max)
        {
            if (_ammoLabel != null)
            {
                _ammoLabel.Text = $"Munitions: {current}/{max}";
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
                tween.TweenProperty(_damageEffect, "color:a", 0.0f, 0.5f);
                tween.TweenCallback(Callable.From(() => _damageEffect.Visible = false));
            }
        }

        /// <summary>
        /// Cache l'interface
        /// </summary>
        public void HideUI()
        {
            Visible = false;
        }

        /// <summary>
        /// Affiche l'interface
        /// </summary>
        public void ShowUI()
        {
            Visible = true;
        }
    }
}
