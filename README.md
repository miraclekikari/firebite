# Firebyte - Cyberpunk FPS Prototype

A 3D multiplayer FPS prototype built with Python and Ursina Engine, set in a futuristic cyberpunk world.

## Features

### Core Gameplay
- **3D Player Controller**: WASD movement, jumping, and mouse-look controls
- **Combat System**: Raycast shooting with ammo management
- **Enemy AI**: Giant boss enemies that track and attack the player
- **Stats System**: Health, Energy, XP, and Leveling mechanics
- **Loot System**: Random item drops with Common, Rare, and Legendary tiers

### Technical Features
- **Modular Architecture**: Clean separation into player.py, weapon.py, enemy.py, and main.py
- **Network Foundation**: Basic multiplayer networking setup for future expansion
- **Futuristic HUD**: Minimalist cyberpunk-themed UI showing HP, Energy, Ammo, and XP

## Installation

1. Install Python 3.8 or higher
2. Install required dependencies:
```bash
pip install -r requirements.txt
```

## Controls

- **WASD**: Move
- **Mouse**: Look around
- **Space**: Jump
- **Left Click**: Shoot
- **R**: Reload weapon
- **E**: Spawn enemy (for testing)
- **Mouse Scroll**: Switch weapons
- **ESC**: Pause/Unpause game

## Game Mechanics

### Player Stats
- **Health**: Player's life points
- **Energy**: Regenerates over time, used for special abilities
- **XP & Leveling**: Gain XP by defeating enemies, level up to increase stats

### Combat
- Weapons have limited ammo and require reloading
- Raycast-based shooting system
- Enemies drop loot when defeated

### Loot System
- **Common** (Gray): Small health/energy/ammo boosts
- **Rare** (Blue): Medium boosts
- **Legendary** (Gold): Large boosts

## File Structure

```
firebyte/
├── main.py          # Main game loop and HUD
├── player.py        # Player controller and stats
├── weapon.py        # Weapon system and combat
├── enemy.py         # Enemy AI and loot system
├── network.py       # Multiplayer networking foundation
├── requirements.txt # Python dependencies
└── README.md       # This file
```

## Running the Game

### Single Player
```bash
python main.py
```

### Multiplayer (Foundation)
The networking foundation is included but not fully integrated. To use:

1. Start a server:
```python
from network import GameNetwork
game_net = GameNetwork()
game_net.start_server()
```

2. Connect as client:
```python
from network import GameNetwork
game_net = GameNetwork()
game_net.connect_to_server('localhost', 5555)
```

## Development Notes

### Tech Stack
- **Ursina Engine**: 3D rendering and game engine
- **Pygame CE**: Additional game functionality
- **Python**: Core programming language

### Architecture
The code follows a modular design pattern:
- Each major system has its own module
- Clear separation of concerns
- Easy to extend and modify

### Future Enhancements
- Full multiplayer integration
- More weapon types
- Advanced enemy behaviors
- Cyberpunk environment assets
- Sound effects and music
- Particle effects
- Save/load system

## Troubleshooting

**Performance Issues**: Lower the render distance or reduce enemy count
**Controls Not Working**: Make sure the game window is focused
**Network Issues**: Check firewall settings and port availability

## License

This project is open source and available under the MIT License.
