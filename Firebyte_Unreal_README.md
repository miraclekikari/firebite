# Firebyte - Cyberpunk FPS (Unreal Engine Version)

An advanced 3D multiplayer FPS prototype built with Unreal Engine 5, featuring a futuristic cyberpunk world with enhanced graphics, AI, and multiplayer capabilities.

## 🚀 Features

### Core Gameplay
- **Advanced Player Controller**: Enhanced movement with sprinting, sliding, and wall-running
- **Realistic Weapon System**: Ballistic physics, multiple weapon types, and advanced recoil
- **Intelligent AI Bosses**: Behavior tree-driven AI with dynamic combat patterns
- **Progression System**: XP, leveling, and skill upgrades
- **Multiplayer Support**: Full network replication for up to 8 players

### Technical Features
- **Unreal Engine 5**: Cutting-edge graphics with Lumen and Nanite
- **Advanced Networking**: Server-authoritative multiplayer with lag compensation
- **Cyberpunk Environment**: Dynamic lighting, weather effects, and atmospheric design
- **UMG HUD**: Modern UI with real-time stats and effects
- **Niagara Effects**: Particle systems for weapons, explosions, and atmosphere

## 🛠️ System Requirements

### Minimum
- **OS**: Windows 10 64-bit
- **Processor**: Intel Core i5-8400 or AMD Ryzen 5 2600
- **Memory**: 12 GB RAM
- **Graphics**: NVIDIA GTX 1060 6GB or AMD RX 580 8GB
- **DirectX**: Version 12
- **Storage**: 25 GB available space

### Recommended
- **OS**: Windows 11 64-bit
- **Processor**: Intel Core i7-10700K or AMD Ryzen 7 3700X
- **Memory**: 16 GB RAM
- **Graphics**: NVIDIA RTX 3070 or AMD RX 6700 XT
- **DirectX**: Version 12
- **Storage**: 25 GB available space (SSD recommended)

## 📦 Installation

1. **Install Unreal Engine 5.3** from Epic Games Launcher
2. **Clone or download** the project files
3. **Open the project** in Unreal Engine:
   - Launch Unreal Engine
   - Click "Browse" and select the `Firebyte.uproject` file
   - The engine will automatically build the project

### Building from Source
```bash
# Navigate to project directory
cd Firebyte

# Generate project files (if needed)
# Right-click Firebyte.uproject and select "Generate Visual Studio project files"

# Build in Visual Studio
# Open Firebyte.sln and build the project
```

## 🎮 Controls

### Movement
- **WASD**: Move
- **Shift**: Sprint
- **Space**: Jump
- **Ctrl**: Crouch/Slide
- **Mouse**: Look around

### Combat
- **Left Click**: Primary Fire
- **Right Click**: Aim/Zoom
- **R**: Reload
- **Q/E**: Lean (when in cover)
- **F**: Interact

### Weapons
- **1-5**: Switch weapons
- **Mouse Wheel**: Cycle weapons
- **G**: Throw grenade
- **V**: Melee attack

### Multiplayer
- **Tab**: Scoreboard
- **J**: Join game
- **K**: Leave game
- **Y**: Chat
- **ESC**: Pause menu

## 🎯 Game Mechanics

### Player Progression
- **Health & Energy**: Regenerating resources
- **XP System**: Gain experience from combat and objectives
- **Leveling**: Unlock new abilities and weapon attachments
- **Skill Tree**: Customize playstyle with upgrades

### Combat System
- **Ballistic Physics**: Realistic bullet trajectories and drop
- **Weapon Variety**: Assault rifles, shotguns, sniper rifles, plasma weapons
- **Cover System**: Dynamic cover with leaning and peeking
- **Melee Combat**: Close-quarters attacks with finishers

### AI Bosses
- **Dynamic Behavior**: AI adapts to player tactics
- **Attack Patterns**: Multiple phases and special abilities
- **Weak Points**: Strategic targeting for maximum damage
- **Environmental Interaction**: Bosses use the environment tactically

### Multiplayer Features
- **Server Architecture**: Dedicated server support with matchmaking
- **Game Modes**: Free-for-all, team deathmatch, boss battles
- **Ranking System**: Competitive matchmaking with ELO ratings
- **Spectator Mode**: Watch matches after elimination

## 🏗️ Project Structure

```
Firebyte/
├── Source/Firebyte/
│   ├── Player/                 # Player controller and character
│   ├── Weapons/               # Weapon system and ballistics
│   ├── AI/                    # AI controllers and behavior
│   ├── Boss/                  # Boss characters and abilities
│   ├── HUD/                   # User interface and widgets
│   ├── Game/                  # Game mode and rules
│   ├── Network/               # Multiplayer networking
│   ├── Effects/               # Particle systems and VFX
│   └── Audio/                 # Sound system and music
├── Content/
│   ├── Maps/                  # Level designs
│   ├── Materials/             # Cyberpunk textures
│   ├── Meshes/                # 3D models
│   ├── Animations/            # Character and weapon animations
│   ├── Effects/               # Niagara particle systems
│   └── Audio/                 # Sound cues and music
└── Config/                    # Engine configuration files
```

## 🔧 Development

### Building the Project
1. Open `Firebyte.sln` in Visual Studio 2022
2. Set build configuration to "Development Editor"
3. Build the solution
4. Run from Unreal Editor

### Key Classes
- **AFBPlayerController**: Main player control and networking
- **AFBWeapon**: Weapon system with ballistics
- **AFBBoss**: AI boss with behavior trees
- **AFBGameMode**: Game rules and multiplayer management
- **UFBHUDWidget**: Modern UMG interface

### Customization
- **Weapons**: Modify `FWeaponStats` in `FBWeapon.h`
- **AI Behaviors**: Edit behavior trees in the AI folder
- **Game Rules**: Adjust `FGameSettings` in `FBGameMode.h`
- **UI Styling**: Customize UMG widgets in the HUD folder

## 🌐 Multiplayer Setup

### Dedicated Server
```bash
# Run dedicated server
FirebyteServer.exe ?Port=7777 ?Map=CyberpunkLevel?Game=FBGameMode
```

### Client Connection
1. Launch the game
2. Open console (`~` key)
3. Type: `open <server_ip>:7777`

### Network Configuration
- **Default Port**: 7777
- **Max Players**: 8 (configurable)
- **Net Mode**: Server-authoritative with client-side prediction

## 🎨 Asset Pipeline

### 3D Models
- **Software**: Blender, Maya, 3ds Max
- **Format**: FBX with proper naming conventions
- **LODs**: Include 3-4 detail levels for optimization

### Textures
- **Resolution**: 2K-4K for hero assets, 1K for environment
- **Format**: BC7 for color, BC5 for normal maps
- **Workflow**: PBR with metallic/roughness workflow

### Audio
- **Format**: WAV for uncompressed, OGG for compressed
- **Sample Rate**: 44.1kHz
- **Channels**: Mono for SFX, Stereo for music

## 🐛 Debugging

### Console Commands
- `showdebug`: Enable debug drawing
- `net pkt`: Show network traffic
- `stat unit`: Performance statistics
- `r.showfps`: Display FPS counter

### Common Issues
- **Network Lag**: Check server tick rate and bandwidth
- **Performance**: Verify LOD settings and culling distances
- **Audio**: Ensure audio cues are properly referenced

## 🚀 Performance Optimization

### Graphics Settings
- **Scalability**: Automatic quality adjustment based on hardware
- **Culling**: Frustum and occlusion culling enabled
- **LODs**: Progressive mesh reduction for distant objects

### Network Optimization
- **Relevance**: Only replicate necessary data
- **Compression**: Enable network compression
- **Prediction**: Client-side movement prediction

## 📄 License

This project is open source under the MIT License. See LICENSE.md for details.

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## 📞 Support

- **Documentation**: Check the Wiki for detailed guides
- **Issues**: Report bugs on GitHub Issues
- **Community**: Join our Discord server
- **Email**: support@firebyte-game.com

---

**Firebyte** - Built with passion for cyberpunk gaming 🌆✨
