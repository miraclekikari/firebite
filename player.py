from ursina import *
import math

class Player(Entity):
    def __init__(self, position=(0, 2, 0)):
        super().__init__(
            model='cube',
            color=color.cyan,
            scale=(1, 2, 1),
            position=position,
            collider='box'
        )
        
        # Movement attributes
        self.speed = 5
        self.jump_height = 2
        self.gravity = -0.5
        self.velocity_y = 0
        self.grounded = False
        
        # Camera setup
        self.camera_pivot = Entity(parent=self, y=2)
        self.camera = Camera(parent=self.camera_pivot, position=(0, 0, -5))
        camera.position = self.position + (0, 2, -5)
        
        # Mouse lock
        self.mouse_sensitivity = Vec2(40, 40)
        mouse.locked = True
        self.original_mouse_position = mouse.position
        
        # Stats
        self.health = 100
        self.max_health = 100
        self.energy = 100
        self.max_energy = 100
        self.xp = 0
        self.level = 1
        self.xp_to_next_level = 100
        
    def update(self):
        # Mouse look
        if mouse.locked:
            self.rotation_y -= mouse.velocity[0] * self.mouse_sensitivity[0] * time.dt
            self.camera_pivot.rotation_x -= mouse.velocity[1] * self.mouse_sensitivity[1] * time.dt
            self.camera_pivot.rotation_x = clamp(self.camera_pivot.rotation_x, -89, 89)
        
        # Movement
        move_direction = Vec3(
            held_keys['d'] - held_keys['a'],
            0,
            held_keys['w'] - held_keys['s']
        ).normalized()
        
        # Apply movement relative to camera direction
        if move_direction.length() > 0:
            move_direction = self.forward * move_direction.z + self.right * move_direction.x
            self.position += move_direction * self.speed * time.dt
        
        # Jumping
        if self.grounded and held_keys['space']:
            self.velocity_y = self.jump_height
            self.grounded = False
        
        # Gravity
        self.velocity_y += self.gravity * time.dt
        self.position += Vec3(0, self.velocity_y * time.dt, 0)
        
        # Ground check
        ray = raycast(self.position, Vec3(0, -1, 0), distance=2.1)
        if ray.hit:
            if self.velocity_y < 0:
                self.position.y = ray.world_point.y + 1.05
                self.velocity_y = 0
                self.grounded = True
        else:
            self.grounded = False
            
        # Energy regeneration
        if self.energy < self.max_energy:
            self.energy = min(self.max_energy, self.energy + 10 * time.dt)
    
    def take_damage(self, damage):
        self.health -= damage
        if self.health <= 0:
            self.health = 0
            self.on_death()
    
    def heal(self, amount):
        self.health = min(self.max_health, self.health + amount)
    
    def use_energy(self, amount):
        if self.energy >= amount:
            self.energy -= amount
            return True
        return False
    
    def gain_xp(self, amount):
        self.xp += amount
        while self.xp >= self.xp_to_next_level:
            self.level_up()
    
    def level_up(self):
        self.xp -= self.xp_to_next_level
        self.level += 1
        self.xp_to_next_level = int(self.xp_to_next_level * 1.5)
        self.max_health += 10
        self.health = self.max_health
        self.max_energy += 5
        self.energy = self.max_energy
        print(f"Level up! Now level {self.level}")
    
    def on_death(self):
        print("Player died!")
        # Respawn logic would go here
    
    def input(self, key):
        if key == 'escape':
            if mouse.locked:
                mouse.locked = False
                application.pause()
            else:
                mouse.locked = True
                application.resume()
