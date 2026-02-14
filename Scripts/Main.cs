using Godot;

namespace Firebyte
{
    /// <summary>
    /// Classe principale du jeu Firebyte - Initialise la scène 3D cyberpunk
    /// </summary>
    public partial class Main : Node3D
    {
        // Références aux nœuds principaux
        private Node3D _worldEnvironment;
        private WorldEnvironment _environment;
        private DirectionalLight3D _sunLight;
        private Player _player;
        private UI _gameUI;
        private TestManager _testManager;

        // Paramètres de l'environnement cyberpunk
        private readonly Color _neonBlue = new Color(0.0f, 0.8f, 1.0f);
        private readonly Color _neonPink = new Color(1.0f, 0.0f, 0.8f);
        private readonly Color _neonGreen = new Color(0.0f, 1.0f, 0.5f);

        public override void _Ready()
        {
            GD.Print("🚀 Initialisation de Firebyte - Cyberpunk FPS");
            
            // Initialiser les composants principaux
            InitializeEnvironment();
            InitializeLighting();
            InitializePlayer();
            InitializeUI();
            InitializeTesting();
            
            GD.Print("✅ Firebyte prêt - Appuyez sur F1 pour lancer les tests");
        }

        /// <summary>
        /// Initialise l'environnement 3D cyberpunk avec effets visuels avancés
        /// </summary>
        private void InitializeEnvironment()
        {
            GD.Print("🏙️ Création de l'environnement cyberpunk avancé...");
            
            // WorldEnvironment avec effets visuels pro
            _worldEnvironment = new Node3D();
            _worldEnvironment.Name = "WorldEnvironment";
            AddChild(_worldEnvironment);

            // Configuration avancée de l'environnement
            SetupAdvancedWorldEnvironment();
            
            // Créer l'environnement
            CreateMetallicFloor();
            CreateCyberpunkBuildings();
            CreateNeonDecorations();
            
            GD.Print("✅ Environnement cyberpunk avancé créé");
        }

        /// <summary>
        /// Configure le WorldEnvironment avec Bloom, SSAO et Fog volumétrique
        /// </summary>
        private void SetupAdvancedWorldEnvironment()
        {
            var worldEnvironment = new WorldEnvironment();
            worldEnvironment.Name = "WorldEnvironment";
            
            var env = new Environment();
            env.BackgroundMode = Environment.BackgroundModeEnum.Color;
            env.BackgroundColor = new Color(0.02f, 0.02f, 0.05f);
            
            // Bloom (Glow) pour l'effet néon
            env.GlowEnabled = true;
            env.GlowIntensity = 1.0f;
            env.GlowStrength = 1.0f;
            env.GlowBloomIntensity = 0.5f;
            env.GlowBlendMode = Environment.GlowBlendModeEnum.Additive;
            env.GlowHdrThreshold = 1.0f;
            env.GlowHdrScale = 2.0f;
            
            // SSAO pour l'occlusion ambiante
            env.SsaoEnabled = true;
            env.SsaoQuality = Environment.SsaoQualityEnum.Ultra;
            env.SsaoBlur = 3;
            env.SsaoEdgeSharpness = 4.0f;
            
            // Fog volumétrique pour l'atmosphère cyberpunk
            env.VolumetricFogEnabled = true;
            env.VolumetricFogDensity = 0.01f;
            env.VolumetricFogAlbedo = new Color(0.0f, 0.8f, 1.0f, 0.3f);
            env.VolumetricFogEmission = 0.2f;
            env.VolumetricFogAnisotropy = 0.2f;
            
            // Brouillard standard
            env.FogEnabled = true;
            env.FogLightColor = new Color(0.0f, 0.8f, 1.0f);
            env.FogDensity = 0.002f;
            env.FogAerialPerspective = 0.1f;
            
            worldEnvironment.Environment = env;
            AddChild(worldEnvironment);
        }

        /// <summary>
        /// Crée le sol métallique avec effet cyberpunk
        /// </summary>
        private void CreateMetallicFloor()
        {
            var floor = new CSGBox3D();
            floor.Name = "MetallicFloor";
            floor.Size = new Vector3(100, 100, 1);
            floor.Position = new Vector3(0, 0, -0.5f);
            
            // Matériau métallique cyberpunk
            var floorMaterial = new StandardMaterial3D();
            floorMaterial.AlbedoColor = new Color(0.2f, 0.2f, 0.3f);
            floorMaterial.Metallic = 0.8f;
            floorMaterial.Roughness = 0.3f;
            floorMaterial.Uv1Scale = new Vector3(20, 20, 1);
            
            floor.Material = floorMaterial;
            _worldEnvironment.AddChild(floor);
        }

        /// <summary>
        /// Crée les bâtiments cyberpunk
        /// </summary>
        private void CreateCyberpunkBuildings()
        {
            // Bâtiment principal
            var mainBuilding = new CSGBox3D();
            mainBuilding.Name = "MainBuilding";
            mainBuilding.Size = new Vector3(15, 15, 40);
            mainBuilding.Position = new Vector3(20, 0, 20);
            
            var buildingMaterial = new StandardMaterial3D();
            buildingMaterial.AlbedoColor = new Color(0.1f, 0.1f, 0.2f);
            buildingMaterial.Metallic = 0.6f;
            buildingMaterial.Roughness = 0.4f;
            mainBuilding.Material = buildingMaterial;
            
            _worldEnvironment.AddChild(mainBuilding);

            // Tours secondaires
            for (int i = 0; i < 3; i++)
            {
                var tower = new CSGBox3D();
                tower.Name = $"Tower_{i}";
                tower.Size = new Vector3(8, 8, 25 + i * 5);
                tower.Position = new Vector3(-30 + i * 15, -25, 12.5f);
                tower.Material = buildingMaterial;
                _worldEnvironment.AddChild(tower);
            }
        }

        /// <summary>
        /// Crée les décorations néon avec shader glow
        /// </summary>
        private void CreateNeonDecorations()
        {
            GD.Print("💡 Création des décorations néon avec shader glow...");
            
            // Créer les piliers néon avec shader
            CreateNeonPillars();
            
            // Créer des panneaux néon
            CreateNeonPanels();
            
            // Créer des câbles lumineux
            CreateNeonCables();
            
            GD.Print("✅ Décorations néon créées");
        }

        /// <summary>
        /// Crée une lumière néon individuelle
        /// </summary>
        private void CreateNeonLight(Vector3 position, Color color, string name)
        {
            var neonLight = new OmniLight3D();
            neonLight.Name = name;
            neonLight.Position = position;
            neonLight.LightColor = color;
            neonLight.LightEnergy = 2.0f;
            neonLight.LightIndirectEnergy = 0.5f;
            neonLight.ShadowEnabled = true;
            
            _worldEnvironment.AddChild(neonLight);
        }

        /// <summary>
        /// Initialise l'éclairage de la scène
        /// </summary>
        private void InitializeLighting()
        {
            GD.Print("💡 Configuration de l'éclairage...");
            
            // Lumière directionnelle principale
            _sunLight = new DirectionalLight3D();
            _sunLight.Name = "SunLight";
            _sunLight.Rotation = new Vector3(-45, 45, 0);
            _sunLight.LightColor = new Color(0.8f, 0.9f, 1.0f);
            _sunLight.LightEnergy = 0.3f;
            _sunLight.ShadowEnabled = true;
            
            AddChild(_sunLight);
            
            // Environnement avec brouillard cyberpunk
            _environment = new WorldEnvironment();
            _environment.Name = "Environment";
            
            var environment = new Environment();
            environment.BackgroundMode = Environment.BackgroundModeEnum.Color;
            environment.BackgroundColor = new Color(0.05f, 0.05f, 0.1f);
            environment.FogEnabled = true;
            environment.FogLightColor = _neonBlue;
            environment.FogDensity = 0.001f;
            
            _environment.Environment = environment;
            AddChild(_environment);
            
            GD.Print("✅ Éclairage configuré");
        }

        /// <summary>
        /// Initialise le joueur
        /// </summary>
        private void InitializePlayer()
        {
            GD.Print("🎮 Initialisation du joueur...");
            
            _player = GD.Load<PackedScene>("res://Scenes/Player.tscn").Instantiate<Player>();
            _player.Name = "Player";
            _player.Position = new Vector3(0, 0, 2);
            
            AddChild(_player);
            
            GD.Print("✅ Joueur initialisé");
        }

        /// <summary>
        /// Initialise l'interface utilisateur
        /// </summary>
        private void InitializeUI()
        {
            GD.Print("🖥️ Initialisation de l'interface...");
            
            _gameUI = GD.Load<PackedScene>("res://Scenes/UI.tscn").Instantiate<UI>();
            _gameUI.Name = "GameUI";
            
            AddChild(_gameUI);
            
            GD.Print("✅ Interface utilisateur initialisée");
        }

        /// <summary>
        /// Initialise le système de test
        /// </summary>
        private void InitializeTesting()
        {
            GD.Print("🧪 Initialisation du système de test...");
            
            _testManager = new TestManager();
            _testManager.Name = "TestManager";
            
            AddChild(_testManager);
            
            GD.Print("✅ Système de test initialisé");
        }

        public override void _Input(InputEvent @event)
        {
            // Touche F1 pour lancer les tests
            if (@event is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.F1)
            {
                GD.Print("🧪 Lancement des tests automatisés...");
                _testManager?.RunAllTests();
            }
            
            // Touche ESC pour quitter
            if (@event is InputEventKey escEvent && escEvent.Pressed && escEvent.Keycode == Key.Escape)
            {
                GetTree().Quit();
            }
        }

        public override void _Process(double delta)
        {
            // Afficher les stats de performance en mode debug
            if (OS.IsDebugBuild())
            {
                Engine.SetMeta("fps", Engine.GetFramesPerSecond());
            }
        }
    }
}
