from ursina import *
import random

class Weapon(Entity):
    def __init__(self, player):
        super().__init__(
            parent=camera.ui,
            model='cube',
            color=color.red,
            scale=(0.3, 0.2, 1),
            position=(0.4, -0.3),
            rotation=(0, 0, 15)
        )
        
        self.player = player
        self.damage = 25
        self.range = 100
        self.fire_rate = 0.1  # seconds between shots
        self.last_shot_time = 0
        
        # Ammo management
        self.current_ammo = 30
        self.max_ammo = 30
        self.reserve_ammo = 90
        self.max_reserve_ammo = 90
        
        # Weapon effects
        self.muzzle_flash = None
        self.impact_effects = []
        
    def update(self):
        # Weapon sway
        self.rotation = Vec3(
            15 + mouse.velocity[1] * 2,
            0 + mouse.velocity[0] * 2,
            0
        )
        
        # Auto-reload
        if self.current_ammo == 0 and self.reserve_ammo > 0:
            self.reload()
    
    def shoot(self):
        current_time = time.time()
        if current_time - self.last_shot_time < self.fire_rate:
            return False
        
        if self.current_ammo <= 0:
            self.reload()
            return False
        
        # Fire the weapon
        self.current_ammo -= 1
        self.last_shot_time = current_time
        
        # Create muzzle flash
        self.create_muzzle_flash()
        
        # Perform raycast
        hit_info = raycast(camera.world_position, camera.forward, distance=self.range)
        
        if hit_info.hit:
            # Create impact effect
            self.create_impact_effect(hit_info.world_point)
            
            # Apply damage if hit entity has health
            if hasattr(hit_info.entity, 'take_damage'):
                hit_info.entity.take_damage(self.damage)
                print(f"Hit {hit_info.entity} for {self.damage} damage!")
            
            # Give player XP for successful hit
            if hasattr(hit_info.entity, 'xp_value'):
                self.player.gain_xp(hit_info.entity.xp_value)
        
        # Weapon recoil
        self.player.camera_pivot.rotation_x += random.uniform(1, 3)
        
        return True
    
    def reload(self):
        if self.reserve_ammo <= 0 or self.current_ammo == self.max_ammo:
            return False
        
        # Calculate ammo to reload
        ammo_needed = self.max_ammo - self.current_ammo
        ammo_to_reload = min(ammo_needed, self.reserve_ammo)
        
        self.current_ammo += ammo_to_reload
        self.reserve_ammo -= ammo_to_reload
        
        print(f"Reloading... {self.current_ammo}/{self.max_ammo}")
        return True
    
    def create_muzzle_flash(self):
        if self.muzzle_flash:
            destroy(self.muzzle_flash)
        
        self.muzzle_flash = Entity(
            parent=self,
            model='quad',
            color=color.yellow,
            scale=0.5,
            position=(0, 0, 0.5),
            eternal=False
        )
        
        # Remove muzzle flash after short duration
        invoke(destroy, self.muzzle_flash, delay=0.05)
    
    def create_impact_effect(self, position):
        impact = Entity(
            model='sphere',
            color=color.orange,
            scale=0.2,
            position=position,
            eternal=False
        )
        
        # Remove impact effect after short duration
        invoke(destroy, impact, delay=0.2)
        
        self.impact_effects.append(impact)
    
    def add_ammo(self, amount):
        self.reserve_ammo = min(self.max_reserve_ammo, self.reserve_ammo + amount)
        print(f"+{amount} ammo added")
    
    def get_ammo_status(self):
        return f"{self.current_ammo}/{self.max_ammo} | {self.reserve_ammo}"

class WeaponManager:
    def __init__(self, player):
        self.player = player
        self.weapons = []
        self.current_weapon_index = 0
        self.current_weapon = None
        
        # Create default weapon
        self.add_weapon(Weapon(player))
        self.equip_weapon(0)
    
    def add_weapon(self, weapon):
        self.weapons.append(weapon)
        weapon.enabled = False
    
    def equip_weapon(self, index):
        if 0 <= index < len(self.weapons):
            if self.current_weapon:
                self.current_weapon.enabled = False
            
            self.current_weapon_index = index
            self.current_weapon = self.weapons[index]
            self.current_weapon.enabled = True
    
    def next_weapon(self):
        self.equip_weapon((self.current_weapon_index + 1) % len(self.weapons))
    
    def previous_weapon(self):
        self.equip_weapon((self.current_weapon_index - 1) % len(self.weapons))
    
    def shoot(self):
        if self.current_weapon:
            return self.current_weapon.shoot()
        return False
    
    def reload(self):
        if self.current_weapon:
            return self.current_weapon.reload()
        return False
    
    def get_current_ammo_status(self):
        if self.current_weapon:
            return self.current_weapon.get_ammo_status()
        return "No weapon"
