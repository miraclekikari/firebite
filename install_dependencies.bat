@echo off
REM Script d'installation des dépendances pour Firebyte FPS (Windows)
echo 🚀 Installation des dépendances pour Firebyte FPS...

REM Vérifier si Godot est installé
where godot >nul 2>nul
if %ERRORLEVEL% NEQ 0 (
    echo ❌ Godot n'est pas installé.
    echo 📥 Veuillez télécharger Godot depuis: https://godotengine.org/download/windows/
    echo    et installer dans C:\Program Files\Godot\
    echo    ou ajouter Godot au PATH système
    pause
    exit /b 1
) else (
    echo ✅ Godot est déjà installé
    godot --version
)

REM Vérifier si .NET SDK est installé
where dotnet >nul 2>nul
if %ERRORLEVEL% NEQ 0 (
    echo ❌ .NET SDK n'est pas installé.
    echo 📥 Veuillez télécharger .NET SDK depuis: https://dotnet.microsoft.com/download
    pause
    exit /b 1
) else (
    echo ✅ .NET SDK est déjà installé
    dotnet --version
)

REM Créer les répertoires nécessaires
echo 📁 Création des répertoires du projet...
if not exist "Scripts" mkdir Scripts
if not exist "Scenes" mkdir Scenes
if not exist "Assets\Materials" mkdir Assets\Materials
if not exist "Assets\Models" mkdir Assets\Models
if not exist "Assets\Sounds" mkdir Assets\Sounds
if not exist "Assets\Textures" mkdir Assets\Textures

REM Créer un fichier de configuration pour les assets
echo 🎨 Création de la configuration des assets...
(
echo {
echo     "cyberpunk_materials": {
echo         "metal_floor": "res://Assets/Materials/metal_floor.tres",
echo         "neon_blue": "res://Assets/Materials/neon_blue.tres",
echo         "neon_pink": "res://Assets/Materials/neon_pink.tres",
echo         "neon_green": "res://Assets/Materials/neon_green.tres"
echo     },
echo     "sounds": {
echo         "shoot": "res://Assets/Sounds/shoot.wav",
echo         "reload": "res://Assets/Sounds/reload.wav",
echo         "hit": "res://Assets/Sounds/hit.wav",
echo         "ambient": "res://Assets/Sounds/cyberpunk_ambient.ogg"
echo     },
echo     "models": {
echo         "player": "res://Assets/Models/player.glb",
echo         "weapon": "res://Assets/Models/weapon.glb",
echo         "target": "res://Assets/Models/target.glb"
echo     }
echo }
) > Assets\asset_config.json

echo ✅ Configuration des assets créée

REM Vérifier le projet Godot
echo 🔍 Vérification du projet Godot...
if exist "project.godot" (
    echo ✅ Fichier project.godot trouvé
    
    REM Lancer Godot pour vérifier le projet
    echo 🎮 Vérification du projet...
    godot --headless --quit
    
    if %ERRORLEVEL% EQU 0 (
        echo ✅ Projet Godot valide!
    ) else (
        echo ❌ Erreur dans le projet Godot
        pause
        exit /b 1
    )
) else (
    echo ❌ Fichier project.godot non trouvé
    pause
    exit /b 1
)

REM Créer un script de lancement
echo 📝 Création du script de lancement...
(
echo @echo off
echo echo 🚀 Lancement de Firebyte FPS...
echo echo 📋 Contrôles:
echo echo    ZQSD: Mouvement
echo echo    Souris: Visée
echo echo    Clic Gauche: Tirer
echo echo    R: Recharger
echo echo    Shift: Sprint
echo echo    Espace: Sauter
echo echo    F1: Lancer les tests
echo echo    ESC: Quitter
echo echo.
echo godot --verbose
) > run_firebyte.bat

REM Créer un script pour l'éditeur
echo 📝 Création du script pour l'éditeur...
(
echo @echo off
echo echo 🛠️ Lancement de l'éditeur Godot...
echo godot --editor
) > open_editor.bat

echo.
echo 🎉 Installation terminée!
echo.
echo 📋 Prochaines étapes:
echo    1. Ouvrir le projet dans Godot: double-cliquer sur open_editor.bat
echo    2. Lancer le jeu: double-cliquer sur run_firebyte.bat
echo    3. Tester avec F1: Tests automatisés
echo.
echo 🔗 Documentation: https://docs.godotengine.org/fr/stable/
echo 💬 Support: https://github.com/votre-repo/firebyte-fps
echo.
echo ✨ Firebyte FPS est prêt!
echo.
pause
