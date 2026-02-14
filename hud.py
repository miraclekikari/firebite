from ursina import *
import math

class HUD:
    def __init__(self, player, weapon_manager):
        self.player = player
        self.weapon_manager = weapon_manager
        
        # Create HUD elements
        self.create_health_bar()
        self.create_energy_bar()
        self.create_crosshair()
        self.create_ammo_display()
        self.create_xp_display()
        self.create_level_display()
        
    def create_health_bar(self):
        # Health bar background
        self.health_bg = Entity(
            parent=camera.ui,
            model='quad',
            color=color.rgba(20, 20, 30, 180),
            scale=(0.3, 0.02),
            position=(-0.65, -0.45),
            origin=(-0.5, -0.5)
        )
        
        # Health bar fill
        self.health_fill = Entity(
            parent=self.health_bg,
            model='quad',
            color=color.rgba(0, 255, 100, 200),
            scale=(1, 1),
            position=(0, 0),
            origin=(-0.5, -0.5)
        )
        
        # Health text
        self.health_text = Text(
            parent=camera.ui,
            text="HP: 100/100",
            position=(-0.65, -0.42),
            scale=0.8,
            color=color.rgba(0, 255, 100, 255),
            origin=(-0.5, -0.5)
        )
    
    def create_energy_bar(self):
        # Energy bar background
        self.energy_bg = Entity(
            parent=camera.ui,
            model='quad',
            color=color.rgba(20, 20, 30, 180),
            scale=(0.3, 0.02),
            position=(-0.65, -0.40),
            origin=(-0.5, -0.5)
        )
        
        # Energy bar fill
        self.energy_fill = Entity(
            parent=self.energy_bg,
            model='quad',
            color=color.rgba(0, 150, 255, 200),
            scale=(1, 1),
            position=(0, 0),
            origin=(-0.5, -0.5)
        )
        
        # Energy text
        self.energy_text = Text(
            parent=camera.ui,
            text="EN: 100/100",
            position=(-0.65, -0.37),
            scale=0.8,
            color=color.rgba(0, 150, 255, 255),
            origin=(-0.5, -0.5)
        )
    
    def create_crosshair(self):
        # Crosshair group
        self.crosshair = Entity(parent=camera.ui)
        
        # Horizontal lines
        self.crosshair_h_left = Entity(
            parent=self.crosshair,
            model='quad',
            color=color.rgba(255, 255, 255, 200),
            scale=(0.01, 0.002),
            position=(-0.015, 0),
            origin=(1, 0.5)
        )
        
        self.crosshair_h_right = Entity(
            parent=self.crosshair,
            model='quad',
            color=color.rgba(255, 255, 255, 200),
            scale=(0.01, 0.002),
            position=(0.015, 0),
            origin=(0, 0.5)
        )
        
        # Vertical lines
        self.crosshair_v_top = Entity(
            parent=self.crosshair,
            model='quad',
            color=color.rgba(255, 255, 255, 200),
            scale=(0.002, 0.01),
            position=(0, 0.015),
            origin=(0.5, 1)
        )
        
        self.crosshair_v_bottom = Entity(
            parent=self.crosshair,
            model='quad',
            color=color.rgba(255, 255, 255, 200),
            scale=(0.002, 0.01),
            position=(0, -0.015),
            origin=(0.5, 0)
        )
        
        # Center dot
        self.crosshair_center = Entity(
            parent=self.crosshair,
            model='circle',
            color=color.rgba(255, 255, 255, 255),
            scale=0.003,
            position=(0, 0)
        )
    
    def create_ammo_display(self):
        # Ammo background
        self.ammo_bg = Entity(
            parent=camera.ui,
            model='quad',
            color=color.rgba(20, 20, 30, 180),
            scale=(0.15, 0.04),
            position=(0.65, -0.45),
            origin=(0.5, -0.5)
        )
        
        # Ammo text
        self.ammo_text = Text(
            parent=camera.ui,
            text="30/90",
            position=(0.65, -0.42),
            scale=1.2,
            color=color.rgba(255, 255, 255, 255),
            origin=(0.5, -0.5)
        )
        
        # Weapon type indicator
        self.weapon_text = Text(
            parent=camera.ui,
            text="PISTOL",
            position=(0.65, -0.38),
            scale=0.7,
            color=color.rgba(200, 200, 200, 255),
            origin=(0.5, -0.5)
        )
    
    def create_xp_display(self):
        # XP bar background
        self.xp_bg = Entity(
            parent=camera.ui,
            model='quad',
            color=color.rgba(20, 20, 30, 180),
            scale=(0.4, 0.015),
            position=(0, -0.48),
            origin=(0, -0.5)
        )
        
        # XP bar fill
        self.xp_fill = Entity(
            parent=self.xp_bg,
            model='quad',
            color=color.rgba(255, 200, 0, 200),
            scale=(0, 1),
            position=(0, 0),
            origin=(-0.5, -0.5)
        )
        
        # XP text
        self.xp_text = Text(
            parent=camera.ui,
            text="XP: 0/100",
            position=(0, -0.46),
            scale=0.7,
            color=color.rgba(255, 200, 0, 255),
            origin=(0, -0.5)
        )
    
    def create_level_display(self):
        # Level text
        self.level_text = Text(
            parent=camera.ui,
            text="LEVEL 1",
            position=(-0.65, 0.45),
            scale=1.0,
            color=color.rgba(255, 255, 255, 255),
            origin=(-0.5, 0.5)
        )
    
    def update(self):
        # Update health bar
        health_percentage = self.player.health / self.player.max_health
        self.health_fill.scale_x = health_percentage
        self.health_text.text = f"HP: {int(self.player.health)}/{self.player.max_health}"
        
        # Change health bar color based on health level
        if health_percentage > 0.6:
            self.health_fill.color = color.rgba(0, 255, 100, 200)
        elif health_percentage > 0.3:
            self.health_fill.color = color.rgba(255, 200, 0, 200)
        else:
            self.health_fill.color = color.rgba(255, 50, 50, 200)
        
        # Update energy bar
        energy_percentage = self.player.energy / self.player.max_energy
        self.energy_fill.scale_x = energy_percentage
        self.energy_text.text = f"EN: {int(self.player.energy)}/{self.player.max_energy}"
        
        # Update ammo display
        if self.weapon_manager and self.weapon_manager.current_weapon:
            ammo_status = self.weapon_manager.get_current_ammo_status()
            self.ammo_text.text = ammo_status
        
        # Update XP bar
        if self.player.xp_to_next_level > 0:
            xp_percentage = self.player.xp / self.player.xp_to_next_level
            self.xp_fill.scale_x = xp_percentage
        self.xp_text.text = f"XP: {self.player.xp}/{self.player.xp_to_next_level}"
        
        # Update level display
        self.level_text.text = f"LEVEL {self.player.level}"
        
        # Update crosshair color based on target
        self.update_crosshair()
    
    def update_crosshair(self):
        # Raycast to check if aiming at enemy
        hit_info = raycast(camera.world_position, camera.forward, distance=100)
        
        if hit_info.hit and hasattr(hit_info.entity, 'take_damage'):
            # Red crosshair when aiming at enemy
            crosshair_color = color.rgba(255, 50, 50, 255)
            # Make crosshair slightly larger
            scale_multiplier = 1.2
        else:
            # White crosshair normally
            crosshair_color = color.rgba(255, 255, 255, 200)
            scale_multiplier = 1.0
        
        # Update crosshair colors
        self.crosshair_h_left.color = crosshair_color
        self.crosshair_h_right.color = crosshair_color
        self.crosshair_v_top.color = crosshair_color
        self.crosshair_v_bottom.color = crosshair_color
        self.crosshair_center.color = crosshair_color
        
        # Update crosshair scale
        self.crosshair_h_left.scale_x = 0.01 * scale_multiplier
        self.crosshair_h_right.scale_x = 0.01 * scale_multiplier
        self.crosshair_v_top.scale_y = 0.01 * scale_multiplier
        self.crosshair_v_bottom.scale_y = 0.01 * scale_multiplier
        self.crosshair_center.scale = 0.003 * scale_multiplier
    
    def show_damage_indicator(self, damage_amount):
        # Create damage number popup
        damage_text = Text(
            parent=camera.ui,
            text=f"-{damage_amount}",
            position=(0, 0.1),
            scale=1.5,
            color=color.rgba(255, 50, 50, 255),
            origin=(0, 0)
        )
        
        # Animate damage text
        damage_text.animate_position((0, 0.2), duration=1.0)
        damage_text.animate_color(color.rgba(255, 50, 50, 0), duration=1.0)
        destroy(damage_text, delay=1.0)
    
    def show_level_up_effect(self):
        # Create level up notification
        level_up_text = Text(
            parent=camera.ui,
            text="LEVEL UP!",
            position=(0, 0),
            scale=2.0,
            color=color.rgba(255, 200, 0, 255),
            origin=(0, 0)
        )
        
        # Animate level up text
        level_up_text.animate_scale(3.0, duration=0.5)
        level_up_text.animate_color(color.rgba(255, 200, 0, 0), duration=1.5, delay=0.5)
        destroy(level_up_text, delay=2.0)
    
    def show_reload_notification(self):
        # Create reload notification
        reload_text = Text(
            parent=camera.ui,
            text="RELOADING",
            position=(0, -0.1),
            scale=1.2,
            color=color.rgba(255, 200, 0, 255),
            origin=(0, 0)
        )
        
        # Animate reload text
        reload_text.animate_color(color.rgba(255, 200, 0, 0), duration=2.0)
        destroy(reload_text, delay=2.0)
    
    def cleanup(self):
        # Clean up all HUD elements
        entities_to_destroy = [
            self.health_bg, self.health_fill, self.health_text,
            self.energy_bg, self.energy_fill, self.energy_text,
            self.crosshair, self.ammo_bg, self.ammo_text, self.weapon_text,
            self.xp_bg, self.xp_fill, self.xp_text, self.level_text
        ]
        
        for entity in entities_to_destroy:
            if hasattr(entity, 'destroy'):
                destroy(entity)
            elif hasattr(entity, 'enabled'):
                entity.enabled = False
