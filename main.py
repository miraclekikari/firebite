from ursina import *
from player import Player
from weapon import WeaponManager
from enemy import Enemy
import random

# Initialize Ursina
app = Ursina()

# Global variables for game state
player = None
weapon_manager = None
enemies = []
hud_elements = {}

# Window settings
window.title = 'Firebyte - Cyberpunk FPS'
window.borderless = False
window.fullscreen = False
window.exit_button.visible = False
window.fps_counter.enabled = True

# Set up the scene
def setup_scene():
    # Sky
    sky = Sky(color=color.dark_gray)
    
    # Ground
    ground = Entity(
        model='cube',
        color=color.dark_gray,
        scale=(100, 1, 100),
        position=(0, 0, 0),
        collider='box',
        texture='white_cube'
    )
    
    # Lighting
    light = DirectionalLight()
    light.look_at(Vec3(1, -1, 1))
    
    # Ambient light
    AmbientLight(color=color.rgba(100, 100, 150, 255))

# Create HUD
def create_hud():
    # Health bar background
    health_bg = Entity(
        parent=camera.ui,
        model='quad',
        color=color.black,
        scale=(0.3, 0.02),
        position=(-0.65, -0.45),
        origin=(-0.5, -0.5)
    )
    
    # Health bar
    health_bar = Entity(
        parent=health_bg,
        model='quad',
        color=color.red,
        scale=(1, 1),
        origin=(-0.5, -0.5)
    )
    
    # Energy bar background
    energy_bg = Entity(
        parent=camera.ui,
        model='quad',
        color=color.black,
        scale=(0.3, 0.02),
        position=(-0.65, -0.40),
        origin=(-0.5, -0.5)
    )
    
    # Energy bar
    energy_bar = Entity(
        parent=energy_bg,
        model='quad',
        color=color.blue,
        scale=(1, 1),
        origin=(-0.5, -0.5)
    )
    
    # Crosshair
    crosshair = Entity(
        parent=camera.ui,
        model='quad',
        color=color.white,
        scale=(0.01, 0.01),
        position=(0, 0)
    )
    
    # Health text
    health_text = Text(
        text='100/100',
        parent=camera.ui,
        position=(-0.65, -0.43),
        scale=1.5,
        color=color.white
    )
    
    # Energy text
    energy_text = Text(
        text='100/100',
        parent=camera.ui,
        position=(-0.65, -0.38),
        scale=1.5,
        color=color.white
    )
    
    # Ammo text
    ammo_text = Text(
        text='30/30 | 90',
        parent=camera.ui,
        position=(0.65, -0.45),
        scale=1.5,
        color=color.white,
        origin=(0.5, 0.5)
    )
    
    # Level and XP text
    level_text = Text(
        text='Level 1',
        parent=camera.ui,
        position=(-0.65, 0.45),
        scale=2,
        color=color.gold
    )
    
    xp_text = Text(
        text='XP: 0/100',
        parent=camera.ui,
        position=(-0.65, 0.40),
        scale=1.5,
        color=color.white
    )
    
    # Store HUD elements
    global hud_elements
    hud_elements = {
        'health_bar': health_bar,
        'energy_bar': energy_bar,
        'health_text': health_text,
        'energy_text': energy_text,
        'ammo_text': ammo_text,
        'level_text': level_text,
        'xp_text': xp_text,
        'crosshair': crosshair
    }

# Update HUD
def update_hud():
    if not player or not weapon_manager:
        return
    
    # Update health bar
    health_percent = player.health / player.max_health
    hud_elements['health_bar'].scale_x = health_percent
    hud_elements['health_text'].text = f'{int(player.health)}/{player.max_health}'
    
    # Update energy bar
    energy_percent = player.energy / player.max_energy
    hud_elements['energy_bar'].scale_x = energy_percent
    hud_elements['energy_text'].text = f'{int(player.energy)}/{player.max_energy}'
    
    # Update ammo text
    ammo_status = weapon_manager.get_current_ammo_status()
    hud_elements['ammo_text'].text = ammo_status
    
    # Update level and XP
    hud_elements['level_text'].text = f'Level {player.level}'
    hud_elements['xp_text'].text = f'XP: {int(player.xp)}/{player.xp_to_next_level}'

# Spawn enemies
def spawn_enemy():
    if len(enemies) < 5:  # Max 5 enemies at once
        # Random position around the player
        angle = random.uniform(0, 360)
        distance = random.uniform(10, 30)
        
        x = player.position.x + distance * math.cos(math.radians(angle))
        z = player.position.z + distance * math.sin(math.radians(angle))
        
        enemy = Enemy(position=(x, 2, z), player=player)
        enemies.append(enemy)
        
        print(f"Spawned enemy at ({x}, 2, {z})")

# Input handling
def input(key):
    global player, weapon_manager
    
    if not player or not weapon_manager:
        return
    
    # Shooting
    if key == 'left mouse down':
        weapon_manager.shoot()
    
    # Reload
    if key == 'r':
        weapon_manager.reload()
    
    # Weapon switching
    if key == 'scroll up':
        weapon_manager.previous_weapon()
    elif key == 'scroll down':
        weapon_manager.next_weapon()
    
    # Spawn enemy for testing
    if key == 'e':
        spawn_enemy()
    
    # Player movement is handled in Player class

# Game update function
def update():
    update_hud()
    
    # Spawn enemies periodically
    if random.random() < 0.005:  # 0.5% chance per frame
        spawn_enemy()
    
    # Remove dead enemies from list
    global enemies
    enemies = [enemy for enemy in enemies if enemy.enabled]

# Initialize game
def init_game():
    global player, weapon_manager, enemies
    
    # Setup scene
    setup_scene()
    
    # Create player
    player = Player(position=(0, 2, 0))
    
    # Create weapon manager
    weapon_manager = WeaponManager(player)
    
    # Create initial enemies
    for i in range(2):
        spawn_enemy()
    
    # Create HUD
    create_hud()
    
    print("Firebyte initialized!")
    print("Controls:")
    print("WASD - Move")
    print("Mouse - Look")
    print("Space - Jump")
    print("Left Click - Shoot")
    print("R - Reload")
    print("E - Spawn Enemy")
    print("Scroll - Switch Weapons")
    print("ESC - Pause/Unpause")

# Run the game
if __name__ == '__main__':
    init_game()
    app.run()
