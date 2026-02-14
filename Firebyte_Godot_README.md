# Firebyte - Cyberpunk FPS (Godot C#)

Un FPS 3D cyberpunk réaliste développé en C# avec Godot Engine, optimisé pour les ressources limitées tout en offrant une expérience de jeu professionnelle.

## 🎯 Fonctionnalités

### Gameplay Principal
- **Mouvements Fluides**: Contrôles ZQSD avec physique réaliste et sprint
- **Tir Raycast Ultra-Rapide**: Système de détection de collision précis et optimisé
- **Stats Complètes**: Santé, Énergie, XP avec progression et niveaux
- **Système d'Armes**: Munitions, rechargement, précision et dégâts
- **Environnement Cyberpunk**: Sol métallique, lumières néon, bâtiments futuristes

### Techniques
- **Architecture C#**: Code propre et modulaire avec design patterns
- **Tests Automatisés**: Suite complète de tests pour validation
- **Interface Cyberpunk**: HUD moderne avec effets visuels
- **Performance Optimisée**: Gestion efficace des ressources

## 🛠️ Installation

### Prérequis
- **Godot Engine 4.2+** (gratuit)
- **.NET SDK 8.0+** (gratuit)
- **Windows/Linux/macOS** supporté

### Installation Automatique

#### Windows
```bash
# Exécuter le script d'installation
./install_dependencies.bat
```

#### Linux/macOS
```bash
# Rendre le script exécutable
chmod +x install_dependencies.sh

# Exécuter l'installation
./install_dependencies.sh
```

### Installation Manuelle

1. **Installer Godot**: Télécharger depuis [godotengine.org](https://godotengine.org)
2. **Installer .NET SDK**: Télécharger depuis [dotnet.microsoft.com](https://dotnet.microsoft.com)
3. **Cloner le projet**: Copier les fichiers du projet
4. **Ouvrir dans Godot**: Double-cliquer sur `project.godot`

## 🎮 Contrôles

### Mouvement
- **Z/Q/S/D**: Mouvement avant/gauche/arrière/droite
- **Souris**: Visée FPS avec lissage
- **Espace**: Saut
- **Shift**: Sprint

### Combat
- **Clic Gauche**: Tirer (Raycast)
- **R**: Recharger l'arme
- **Mollette**: Changer d'arme (futur)

### Interface
- **F1**: Lancer les tests automatisés
- **ESC**: Quitter le jeu

## 🧪 Tests Automatisés

Firebyte inclut une suite complète de tests automatisés:

### Tests Disponibles
- ✅ **Mouvement du joueur**: Validation des contrôles ZQSD
- ✅ **Tir Raycast**: Vérification de la détection de collision
- ✅ **Système de dégâts**: Application correcte des dégâts
- ✅ **StatsManager**: Gestion de santé, énergie et XP
- ✅ **WeaponManager**: Munitions et rechargement
- ✅ **Système d'XP**: Progression et niveaux
- ✅ **Régénération**: Santé et énergie automatiques
- ✅ **Précision**: Suivi des tirs réussis

### Lancer les Tests
```bash
# Dans le jeu, appuyer sur F1
# Ou via la console:
godot --headless --script Scripts/GameTest.cs
```

## 📁 Structure du Projet

```
Firebyte/
├── Scripts/                 # Code C# du jeu
│   ├── Main.cs             # Scène principale et environnement
│   ├── Player.cs           # Contrôleur du joueur FPS
│   ├── StatsManager.cs     # Gestion des stats (Santé, Énergie, XP)
│   ├── WeaponManager.cs    # Système d'armes et munitions
│   ├── GameTest.cs        # Tests automatisés
│   └── UI.cs              # Interface cyberpunk
├── Scenes/                 # Scènes Godot (.tscn)
├── Assets/                 # Ressources du jeu
│   ├── Materials/         # Matériaux cyberpunk
│   ├── Models/           # Modèles 3D
│   ├── Sounds/           # Sons et musique
│   └── Textures/        # Textures PBR
├── project.godot           # Configuration du projet
├── install_dependencies.bat # Installation Windows
└── install_dependencies.sh # Installation Linux/macOS
```

## 🎨 Personnalisation

### Modifier les Stats du Joueur
Dans `Scripts/StatsManager.cs`:
```csharp
[Export] public float MaxHealth { get; set; } = 100.0f;
[Export] public float MaxEnergy { get; set; } = 100.0f;
[Export] public float HealthRegenerationRate { get; set; } = 2.0f;
```

### Modifier l'Arme
Dans `Scripts/WeaponManager.cs`:
```csharp
[Export] public int MaxAmmo { get; set; } = 30;
[Export] public float FireRate { get; set; } = 600.0f;
[Export] public float BaseDamage { get; set; } = 25.0f;
```

### Personnaliser l'Environnement
Dans `Scripts/Main.cs`:
```csharp
private void CreateCyberpunkBuildings()
{
    // Ajouter vos propres bâtiments ici
}
```

## 🔧 Développement

### Compiler le Projet
```bash
# Ouvrir Godot Editor
godot --editor

# Le projet se compile automatiquement au lancement
```

### Débogage
```bash
# Lancer avec sortie verbose
godot --verbose

# Lancer en mode headless (serveur)
godot --headless
```

### Ajouter de Nouveaux Tests
Dans `Scripts/GameTest.cs`:
```csharp
private void TestNewFeature()
{
    RunTest("Nouveau test", () => {
        // Votre logique de test ici
        return TestResult.Pass("Test réussi");
    });
}
```

## 🚀 Performance

### Optimisations Incluses
- **Raycast Optimisé**: Utilisation efficace de l'espace 3D
- **Gestion des Timers**: Pas de gaspillage CPU
- **UI Légère**: Interface optimisée pour 60 FPS
- **Mémoire**: Nettoyage automatique des ressources

### Recommandations
- **60 FPS Cible**: Configuré pour gameplay fluide
- **Résolution**: 1920x1080 recommandé
- **GPU**: Carte graphique basique suffisante
- **RAM**: 4GB minimum recommandé

## 🐛 Dépannage

### Problèmes Communs
- **Godot non trouvé**: Installer Godot et ajouter au PATH
- **.NET manquant**: Installer .NET SDK 8.0+
- **Tests échouent**: Vérifier la console pour erreurs spécifiques
- **Performance faible**: Baisser la résolution ou désactiver les effets

### Logs et Debug
```bash
# Activer les logs détaillés
godot --verbose --log-file firebyte.log

# Vérifier les erreurs de compilation
godot --headless --quit
```

## 🌟 Fonctionnalités Futures

### Roadmap
- [ ] **Multijoueur**: Réseau et matchmaking
- [ ] **Plus d'Armes**: Shotgun, sniper, laser
- [ ] **Ennemis IA**: Boss et comportements avancés
- [ ] **Niveaux**: Plusieurs maps cyberpunk
- [ ] **Customisation**: Skins et améliorations
- [ ] **Son**: Musique et effets sonores
- [ ] **Sauvegarde**: Progression du joueur

### Contribuer
1. Fork le projet
2. Créer une branche de fonctionnalité
3. Ajouter des tests pour les nouvelles fonctionnalités
4. Soumettre une Pull Request

## 📄 Licence

Ce projet est open source sous licence MIT. Voir `LICENSE.md` pour détails.

## 🤝 Support

- **Documentation**: [Wiki du projet](https://github.com/votre-repo/firebyte-fps/wiki)
- **Issues**: [Signaler des bugs](https://github.com/votre-repo/firebyte-fps/issues)
- **Discord**: [Serveur communautaire](https://discord.gg/firebyte)
- **Email**: support@firebyte-game.com

---

**Firebyte FPS** - L'avenir du cyberpunk gaming 🌆✨

*Développé avec ❤️ et C# pour Godot Engine*
