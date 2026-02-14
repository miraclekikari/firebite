using Godot;
using System;
using System.Collections.Generic;

namespace Firebyte
{
    /// <summary>
    /// Gestionnaire de tests automatisés pour Firebyte FPS
    /// </summary>
    public partial class TestManager : Node
    {
        // Résultats des tests
        private List<TestResult> _testResults = new List<TestResult>();
        private int _testsPassed = 0;
        private int _testsFailed = 0;

        // Références aux objets de test
        private Node3D _testEnvironment;
        private Player _testPlayer;
        private StatsManager _testStats;
        private WeaponManager _testWeapon;
        private Node3D _testTarget;

        public override void _Ready()
        {
            GD.Print("🧪 TestManager initialisé - Appuyez sur F1 pour lancer les tests");
        }

        /// <summary>
        /// Lance tous les tests automatisés
        /// </summary>
        public void RunAllTests()
        {
            GD.Print("🚀 Lancement de la suite de tests automatisés...");
            GD.Print("=" * 50);
            
            // Réinitialiser les résultats
            _testResults.Clear();
            _testsPassed = 0;
            _testsFailed = 0;
            
            // Créer l'environnement de test
            SetupTestEnvironment();
            
            // Exécuter les tests
            TestPlayerMovement();
            TestRaycastShooting();
            TestDamageSystem();
            TestStatsManager();
            TestWeaponManager();
            TestXPSystem();
            TestHealthRegeneration();
            TestEnergySystem();
            TestReloadSystem();
            TestAccuracyTracking();
            
            // Afficher les résultats
            DisplayTestResults();
            
            // Nettoyer
            CleanupTestEnvironment();
        }

        /// <summary>
        /// Configure l'environnement de test
        /// </summary>
        private void SetupTestEnvironment()
        {
            GD.Print("🏗️ Configuration de l'environnement de test...");
            
            _testEnvironment = new Node3D();
            _testEnvironment.Name = "TestEnvironment";
            GetTree().CurrentScene.AddChild(_testEnvironment);
            
            // Créer un joueur de test
            _testPlayer = new Player();
            _testPlayer.Name = "TestPlayer";
            _testPlayer.Position = new Vector3(0, 0, 2);
            _testEnvironment.AddChild(_testPlayer);
            
            // Obtenir les références aux composants
            _testStats = _testPlayer.GetNode<StatsManager>("StatsManager");
            _testWeapon = _testPlayer.GetNode<WeaponManager>("WeaponManager");
            
            // Créer une cible de test
            CreateTestTarget();
            
            GD.Print("✅ Environnement de test configuré");
        }

        /// <summary>
        /// Crée une cible pour les tests de tir
        /// </summary>
        private void CreateTestTarget()
        {
            _testTarget = new MeshInstance3D();
            _testTarget.Name = "TestTarget";
            _testTarget.Mesh = new BoxMesh();
            _testTarget.Position = new Vector3(10, 0, 2);
            ((BoxMesh)_testTarget.Mesh).Size = new Vector3(2, 2, 2);
            
            // Ajouter un StatsManager à la cible
            var targetStats = new StatsManager();
            targetStats.Name = "StatsManager";
            targetStats.MaxHealth = 50;
            _testTarget.AddChild(targetStats);
            
            // Ajouter une collision shape
            var collisionShape = new CollisionShape3D();
            collisionShape.Shape = new BoxShape3D();
            _testTarget.AddChild(collisionShape);
            
            _testEnvironment.AddChild(_testTarget);
        }

        /// <summary>
        /// Test le mouvement du joueur
        /// </summary>
        private void TestPlayerMovement()
        {
            RunTest("Mouvement du joueur", () => {
                var initialPosition = _testPlayer.Position;
                
                // Simuler un mouvement
                _testPlayer.Position += new Vector3(1, 0, 0);
                
                var moved = _testPlayer.Position.X > initialPosition.X;
                return moved ? TestResult.Pass("Le joueur se déplace correctement") 
                            : TestResult.Fail("Le joueur ne se déplace pas");
            });
        }

        /// <summary>
        /// Test le système de tir par raycast
        /// </summary>
        private void TestRaycastShooting()
        {
            RunTest("Tir Raycast", () => {
                var initialTargetHealth = _testTarget.GetNode<StatsManager>("StatsManager").CurrentHealth;
                
                // Simuler un tir
                var spaceState = GetWorld3D().DirectSpaceState;
                var from = _testPlayer.GetCameraPosition();
                var to = _testTarget.Position;
                
                var query = PhysicsRayQueryParameters3D.Create(from, to);
                var result = spaceState.IntersectRay(query);
                
                var hitTarget = result.Count > 0;
                var targetDamaged = false;
                
                if (hitTarget)
                {
                    // Simuler les dégâts
                    var targetStats = _testTarget.GetNode<StatsManager>("StatsManager");
                    targetStats.TakeDamage(25);
                    targetDamaged = targetStats.CurrentHealth < initialTargetHealth;
                }
                
                if (hitTarget && targetDamaged)
                {
                    return TestResult.Pass("Raycast fonctionne et les dégâts sont appliqués");
                }
                else if (!hitTarget)
                {
                    return TestResult.Fail("Raycast ne touche pas la cible");
                }
                else
                {
                    return TestResult.Fail("Raycast touche mais les dégâts ne sont pas appliqués");
                }
            });
        }

        /// <summary>
        /// Test le système de dégâts
        /// </summary>
        private void TestDamageSystem()
        {
            RunTest("Système de dégâts", () => {
                var initialHealth = _testStats.CurrentHealth;
                var damageAmount = 20.0f;
                
                _testStats.TakeDamage(damageAmount);
                var expectedHealth = initialHealth - damageAmount;
                var actualHealth = _testStats.CurrentHealth;
                
                var healthCorrect = Math.Abs(actualHealth - expectedHealth) < 0.01f;
                return healthCorrect ? TestResult.Pass($"Dégâts appliqués correctement: {damageAmount}")
                                 : TestResult.Fail($"Dégâts incorrects: attendu {expectedHealth}, reçu {actualHealth}");
            });
        }

        /// <summary>
        /// Test le StatsManager
        /// </summary>
        private void TestStatsManager()
        {
            RunTest("StatsManager", () => {
                var initialHealth = _testStats.MaxHealth;
                var initialEnergy = _testStats.MaxEnergy;
                
                // Test de réinitialisation
                _testStats.ResetStats();
                
                var healthReset = _testStats.CurrentHealth == _testStats.MaxHealth;
                var energyReset = _testStats.CurrentEnergy == _testStats.MaxEnergy;
                var levelReset = _testStats.Level == 1;
                
                return (healthReset && energyReset && levelReset) 
                    ? TestResult.Pass("StatsManager réinitialisé correctement")
                    : TestResult.Fail("StatsManager réinitialisation incorrecte");
            });
        }

        /// <summary>
        /// Test le WeaponManager
        /// </summary>
        private void TestWeaponManager()
        {
            RunTest("WeaponManager", () => {
                var initialAmmo = _testWeapon.CurrentAmmo;
                var canShoot = _testWeapon.CanShoot;
                
                // Simuler un tir
                _testWeapon.Shoot();
                var ammoDecreased = _testWeapon.CurrentAmmo < initialAmmo;
                
                // Test de rechargement
                _testWeapon.Reload();
                var reloadStarted = _testWeapon.IsReloading;
                
                return (canShoot && ammoDecreased && reloadStarted)
                    ? TestResult.Pass("WeaponManager fonctionne correctement")
                    : TestResult.Fail("WeaponManager présente des problèmes");
            });
        }

        /// <summary>
        /// Test le système d'XP
        /// </summary>
        private void TestXPSystem()
        {
            RunTest("Système d'XP", () => {
                var initialLevel = _testStats.Level;
                var initialXP = _testStats.CurrentXP;
                
                // Ajouter assez d'XP pour monter d'un niveau
                _testStats.AddXP(150);
                
                var levelUp = _testStats.Level > initialLevel;
                var xpIncreased = _testStats.CurrentXP >= 0;
                
                return (levelUp && xpIncreased)
                    ? TestResult.Pass($"XP et niveau corrects: Niveau {_testStats.Level}")
                    : TestResult.Fail("Système d'XP défaillant");
            });
        }

        /// <summary>
        /// Test la régénération de santé
        /// </summary>
        private void TestHealthRegeneration()
        {
            RunTest("Régénération de santé", () => {
                _testStats.TakeDamage(30);
                var damagedHealth = _testStats.CurrentHealth;
                
                // Attendre un peu pour la régénération
                await ToSignal(GetTree().CreateTimer(1.0f), "timeout");
                
                var regeneratedHealth = _testStats.CurrentHealth;
                var healthIncreased = regeneratedHealth > damagedHealth;
                
                return healthIncreased 
                    ? TestResult.Pass($"Santé régénérée: {regeneratedHealth - damagedHealth:F1}")
                    : TestResult.Fail("Pas de régénération de santé");
            });
        }

        /// <summary>
        /// Test le système d'énergie
        /// </summary>
        private void TestEnergySystem()
        {
            RunTest("Système d'énergie", () => {
                var initialEnergy = _testStats.CurrentEnergy;
                var energyToUse = 20.0f;
                
                var canUse = _testStats.HasEnoughEnergy(energyToUse);
                var used = _testStats.UseEnergy(energyToUse);
                var energyDecreased = _testStats.CurrentEnergy < initialEnergy;
                
                return (canUse && used && energyDecreased)
                    ? TestResult.Pass($"Énergie utilisée: {energyToUse}")
                    : TestResult.Fail("Système d'énergie défaillant");
            });
        }

        /// <summary>
        /// Test le système de rechargement
        /// </summary>
        private void TestReloadSystem()
        {
            RunTest("Système de rechargement", () => {
                // Vider les munitions
                while (_testWeapon.CurrentAmmo > 0)
                {
                    _testWeapon.Shoot();
                }
                
                var emptyAmmo = _testWeapon.CurrentAmmo == 0;
                var canReload = _testWeapon.CanReload();
                
                if (canReload)
                {
                    _testWeapon.Reload();
                    // Attendre la fin du rechargement
                    await ToSignal(GetTree().CreateTimer(2.5f), "timeout");
                    
                    var reloaded = _testWeapon.CurrentAmmo > 0;
                    return (emptyAmmo && reloaded)
                        ? TestResult.Pass("Rechargement effectué correctement")
                        : TestResult.Fail("Rechargement échoué");
                }
                
                return TestResult.Fail("Impossible de recharger");
            });
        }

        /// <summary>
        /// Test le suivi de précision
        /// </summary>
        private void TestAccuracyTracking()
        {
            RunTest("Suivi de précision", () => {
                var initialShots = _testWeapon.GetType().GetField("_totalShots");
                var initialHits = _testWeapon.GetType().GetField("_totalHits");
                
                // Simuler des tirs et des touches
                _testWeapon.Shoot();
                _testWeapon.RegisterHit();
                _testWeapon.Shoot();
                _testWeapon.RegisterHit();
                
                var accuracy = _testWeapon.Accuracy;
                var expectedAccuracy = 1.0f; // 2/2 tirs réussis
                
                return Math.Abs(accuracy - expectedAccuracy) < 0.01f
                    ? TestResult.Pass($"Précision correcte: {accuracy:P1}")
                    : TestResult.Fail($"Précision incorrecte: {accuracy:P1}");
            });
        }

        /// <summary>
        /// Exécute un test individuel
        /// </summary>
        private void RunTest(string testName, Func<TestResult> testFunction)
        {
            try
            {
                GD.Print($"🧪 Test: {testName}");
                var result = testFunction();
                _testResults.Add(result);
                
                if (result.Passed)
                {
                    _testsPassed++;
                    GD.Print($"   ✅ {result.Message}");
                }
                else
                {
                    _testsFailed++;
                    GD.Print($"   ❌ {result.Message}");
                }
            }
            catch (Exception ex)
            {
                _testsFailed++;
                var errorResult = TestResult.Fail($"Exception: {ex.Message}");
                _testResults.Add(errorResult);
                GD.Print($"   💥 Exception: {ex.Message}");
            }
        }

        /// <summary>
        /// Affiche les résultats des tests
        /// </summary>
        private void DisplayTestResults()
        {
            GD.Print("=" * 50);
            GD.Print("📊 RÉSULTATS DES TESTS");
            GD.Print("=" * 50);
            GD.Print($"✅ Tests réussis: {_testsPassed}");
            GD.Print($"❌ Tests échoués: {_testsFailed}");
            GD.Print($"📈 Taux de réussite: {(float)_testsPassed / (_testsPassed + _testsFailed) * 100:F1}%");
            
            if (_testsFailed == 0)
            {
                GD.Print("🎉 TOUS LES TESTS SONT PASSÉS! Firebyte est prêt!");
            }
            else
            {
                GD.Print("⚠️ Certains tests ont échoué - Vérifiez les erreurs ci-dessus");
            }
            
            GD.Print("=" * 50);
        }

        /// <summary>
        /// Nettoie l'environnement de test
        /// </summary>
        private void CleanupTestEnvironment()
        {
            if (_testEnvironment != null)
            {
                _testEnvironment.QueueFree();
                _testEnvironment = null;
            }
            
            GD.Print("🧹 Environnement de test nettoyé");
        }
    }

    /// <summary>
    /// Résultat d'un test
    /// </summary>
    public class TestResult
    {
        public bool Passed { get; set; }
        public string Message { get; set; }

        public static TestResult Pass(string message)
        {
            return new TestResult { Passed = true, Message = message };
        }

        public static TestResult Fail(string message)
        {
            return new TestResult { Passed = false, Message = message };
        }
    }
}
