#!/bin/bash

# Script d'installation des dépendances pour Firebyte FPS Godot
echo "🚀 Installation des dépendances pour Firebyte FPS..."

# Vérifier si Godot est installé
if ! command -v godot &> /dev/null; then
    echo "❌ Godot n'est pas installé. Installation en cours..."
    
    # Installation de Godot (Linux)
    if [[ "$OSTYPE" == "linux-gnu"* ]]; then
        wget -O godot.zip "https://downloads.tuxfamily.org/godotengine/4.2/godot-4.2-stable-linux.x86_64.zip"
        unzip godot.zip -d godot
        sudo mv godot/godot-4.2-stable-linux.x86_64 /usr/local/bin/godot
        rm -rf godot godot.zip
        
    # Installation de Godot (macOS)
    elif [[ "$OSTYPE" == "darwin"* ]]; then
        brew install godot
        
    # Installation de Godot (Windows)
    elif [[ "$OSTYPE" == "msys" ]] || [[ "$OSTYPE" == "cygwin" ]]; then
        echo "📥 Veuillez télécharger Godot depuis https://godotengine.org/download/windows/"
        echo "   et l'installer dans C:\\Program Files\\Godot"
    fi
else
    echo "✅ Godot est déjà installé: $(godot --version)"
fi

# Vérifier .NET SDK
if ! command -v dotnet &> /dev/null; then
    echo "❌ .NET SDK n'est pas installé. Installation en cours..."
    
    # Installation de .NET (Linux)
    if [[ "$OSTYPE" == "linux-gnu"* ]]; then
        wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
        sudo dpkg -i packages-microsoft-prod.deb
        sudo apt-get update
        sudo apt-get install -y dotnet-sdk-8.0
        
    # Installation de .NET (macOS)
    elif [[ "$OSTYPE" == "darwin"* ]]; then
        brew install dotnet
        
    # Installation de .NET (Windows)
    elif [[ "$OSTYPE" == "msys" ]] || [[ "$OSTYPE" == "cygwin" ]]; then
        echo "📥 Veuillez télécharger .NET SDK depuis https://dotnet.microsoft.com/download"
    fi
else
    echo "✅ .NET SDK est déjà installé: $(dotnet --version)"
fi

# Créer les répertoires nécessaires
echo "📁 Création des répertoires du projet..."
mkdir -p Scripts
mkdir -p Scenes
mkdir -p Assets/Materials
mkdir -p Assets/Models
mkdir -p Assets/Sounds
mkdir -p Assets/Textures

# Télécharger les assets de base (optionnel)
echo "🎨 Téléchargement des assets cyberpunk de base..."

# Créer un fichier de configuration pour les assets
cat > Assets/asset_config.json << EOF
{
    "cyberpunk_materials": {
        "metal_floor": "res://Assets/Materials/metal_floor.tres",
        "neon_blue": "res://Assets/Materials/neon_blue.tres",
        "neon_pink": "res://Assets/Materials/neon_pink.tres",
        "neon_green": "res://Assets/Materials/neon_green.tres"
    },
    "sounds": {
        "shoot": "res://Assets/Sounds/shoot.wav",
        "reload": "res://Assets/Sounds/reload.wav",
        "hit": "res://Assets/Sounds/hit.wav",
        "ambient": "res://Assets/Sounds/cyberpunk_ambient.ogg"
    },
    "models": {
        "player": "res://Assets/Models/player.glb",
        "weapon": "res://Assets/Models/weapon.glb",
        "target": "res://Assets/Models/target.glb"
    }
}
EOF

echo "✅ Configuration des assets créée"

# Vérifier le projet Godot
echo "🔍 Vérification du projet Godot..."
if [ -f "project.godot" ]; then
    echo "✅ Fichier project.godot trouvé"
    
    # Lancer Godot pour vérifier le projet
    echo "🎮 Lancement de Godot pour vérifier le projet..."
    godot --headless --quit
    
    if [ $? -eq 0 ]; then
        echo "✅ Projet Godot valide!"
    else
        echo "❌ Erreur dans le projet Godot"
        exit 1
    fi
else
    echo "❌ Fichier project.godot non trouvé"
    exit 1
fi

# Créer un script de lancement
cat > run_firebyte.sh << 'EOF'
#!/bin/bash
echo "🚀 Lancement de Firebyte FPS..."
echo "📋 Contrôles:"
echo "   ZQSD: Mouvement"
echo "   Souris: Visée"
echo "   Clic Gauche: Tirer"
echo "   R: Recharger"
echo "   Shift: Sprint"
echo "   Espace: Sauter"
echo "   F1: Lancer les tests"
echo "   ESC: Quitter"
echo ""
godot --verbose
EOF

chmod +x run_firebyte.sh

echo ""
echo "🎉 Installation terminée!"
echo ""
echo "📋 Prochaines étapes:"
echo "   1. Ouvrir le projet dans Godot: godot --editor"
echo "   2. Lancer le jeu: ./run_firebyte.sh"
echo "   3. Tester avec F1: Tests automatisés"
echo ""
echo "🔗 Documentation: https://docs.godotengine.org/fr/stable/"
echo "💬 Support: https://github.com/votre-repo/firebyte-fps"
echo ""
echo "✨ Firebyte FPS est prêt!"
