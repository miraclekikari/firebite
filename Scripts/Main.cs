using Godot;
using System;

namespace Firebyte
{
    /// <summary>
    /// Scène principale finale compatible Godot 4.2
    /// </summary>
    public partial class Main : Node3D
    {
        // Références aux composants principaux
        private WorldEnvironment _worldEnvironment;
        private Player _player;
        private UI _gameUI;

        public override void _Ready()
        {
            GD.Print("🌆 Initialisation de la scène principale Firebyte...");
            
            // Initialiser les composants
            InitializeEnvironment();
            InitializePlayer();
            InitializeUI();
            CreateWorld();
            
            GD.Print("✅ Scène principale initialisée");
        }

        /// <summary>
        /// Initialise l'environnement de base
        /// </summary>
        private void InitializeEnvironment()
        {
            GD.Print("🌍 Configuration de l'environnement...");
            
            // Créer le WorldEnvironment
            _worldEnvironment = new WorldEnvironment();
            _worldEnvironment.Name = "WorldEnvironment";
            AddChild(_worldEnvironment);

            // Configurer l'environnement de base
            var environment = new Godot.Environment();
            environment.BackgroundMode = Godot.Environment.BGMode.Color;
            environment.BackgroundColor = new Color(0.02f, 0.02f, 0.05f, 1.0f);
            environment.AmbientLightColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
            
            // Effets de base
            environment.FogEnabled = true;
            environment.FogLightColor = new Color(0.0f, 0.8f, 1.0f, 1.0f);
            environment.FogDensity = 0.002f;
            environment.GlowEnabled = true;
            environment.GlowIntensity = 1.0f;
            environment.GlowStrength = 1.0f;

            _worldEnvironment.Environment = environment;
            
            GD.Print("✅ Environnement configuré");
        }

        /// <summary>
        /// Initialise le joueur
        /// </summary>
        private void InitializePlayer()
        {
            GD.Print("🎮 Initialisation du joueur...");
            
            _player = new Player();
            _player.Name = "Player";
            _player.Position = new Vector3(0, 0, 2);
            AddChild(_player);
            
            // Ajouter au groupe pour la détection
            _player.AddToGroup("player");
            
            GD.Print("✅ Joueur initialisé");
        }

        /// <summary>
        /// Initialise l'interface utilisateur
        /// </summary>
        private void InitializeUI()
        {
            GD.Print("🖥️ Initialisation de l'interface...");
            
            _gameUI = new UI();
            _gameUI.Name = "UI";
            AddChild(_gameUI);
            
            GD.Print("✅ Interface initialisée");
        }

        /// <summary>
        /// Crée le monde de jeu de base
        /// </summary>
        private void CreateWorld()
        {
            GD.Print("🏗️ Création du monde...");
            
            // Sol métallique
            CreateMetallicFloor();
            
            // Éclairage principal
            CreateMainLighting();
            
            GD.Print("✅ Monde créé");
        }

        /// <summary>
        /// Crée le sol métallique
        /// </summary>
        private void CreateMetallicFloor()
        {
            var floor = new CsgBox3D();
            floor.Name = "MetallicFloor";
            floor.Size = new Vector3(100, 100, 1);
            floor.Position = new Vector3(0, 0, -0.5f);
            
            // Matériau métallique
            var material = new StandardMaterial3D();
            material.AlbedoColor = new Color(0.15f, 0.15f, 0.25f, 1.0f);
            material.Metallic = 0.9f;
            material.Roughness = 0.1f;
            material.Uv1Scale = new Vector3(50, 50, 1);
            floor.Material = material;
            
            AddChild(floor);
        }

        /// <summary>
        /// Crée l'éclairage principal
        /// </summary>
        private void CreateMainLighting()
        {
            // Lumière directionnelle principale
            var directionalLight = new DirectionalLight3D();
            directionalLight.Name = "DirectionalLight3D";
            directionalLight.Rotation = new Vector3(-45, 45, 0);
            directionalLight.LightColor = new Color(0.9f, 0.95f, 1.0f, 1.0f);
            directionalLight.LightEnergy = 0.2f;
            directionalLight.ShadowEnabled = true;
            directionalLight.ShadowBias = 0.5f;
            AddChild(directionalLight);
        }

        /// <summary>
        /// Point d'entrée principal pour les tests
        /// </summary>
        public void StartGame()
        {
            GD.Print("🚀 Démarrage du jeu Firebyte!");
            
            // Activer le joueur
            if (_player != null)
            {
                _player.SetProcess(true);
                _player.SetPhysicsProcess(true);
            }
            
            // Afficher l'interface
            if (_gameUI != null)
            {
                _gameUI.Visible = true;
            }
        }

        /// <summary>
        /// Nettoie les ressources
        /// </summary>
        public override void _ExitTree()
        {
            GD.Print("🧹 Nettoyage de la scène principale...");
            
            // Nettoyer les références
            _worldEnvironment = null;
            _player = null;
            _gameUI = null;
        }
    }
}
