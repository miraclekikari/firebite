from ursina import *
import random
import math

class Enemy(Entity):
    def __init__(self, position=(10, 2, 10), player=None):
        super().__init__(
            model='cube',
            color=color.red,
            scale=(3, 5, 3),  # Giant boss size
            position=position,
            collider='box'
        )
        
        self.player = player
        
        # Enemy stats
        self.health = 500
        self.max_health = 500
        self.damage = 10
        self.speed = 2
        self.attack_range = 5
        self.detection_range = 30
        
        # AI behavior
        self.state = 'idle'  # idle, chasing, attacking
        self.last_attack_time = 0
        self.attack_cooldown = 2.0
        
        # Movement
        self.velocity = Vec3(0, 0, 0)
        self.gravity = -0.5
        self.velocity_y = 0
        self.grounded = False
        
        # Visual effects
        self.original_color = self.color
        self.hit_flash_duration = 0.1
        self.hit_flash_time = 0
        
        # XP value for player
        self.xp_value = 100
        
        # Loot drops
        self.loot_table = {
            'common': 0.6,
            'rare': 0.3,
            'legendary': 0.1
        }
    
    def update(self):
        # Update AI state
        self.update_ai_state()
        
        # Execute behavior based on state
        if self.state == 'idle':
            self.idle_behavior()
        elif self.state == 'chasing':
            self.chase_behavior()
        elif self.state == 'attacking':
            self.attack_behavior()
        
        # Apply gravity
        self.apply_gravity()
        
        # Update hit flash
        if self.hit_flash_time > 0:
            self.hit_flash_time -= time.dt
            if self.hit_flash_time <= 0:
                self.color = self.original_color
    
    def update_ai_state(self):
        if not self.player:
            return
        
        distance_to_player = distance(self.position, self.player.position)
        
        if distance_to_player <= self.detection_range:
            if distance_to_player <= self.attack_range:
                self.state = 'attacking'
            else:
                self.state = 'chasing'
        else:
            self.state = 'idle'
    
    def idle_behavior(self):
        # Random wandering when idle
        if random.random() < 0.01:  # 1% chance per frame
            random_direction = Vec3(
                random.uniform(-1, 1),
                0,
                random.uniform(-1, 1)
            ).normalized()
            self.velocity = random_direction * self.speed * 0.5
    
    def chase_behavior(self):
        if not self.player:
            return
        
        # Calculate direction to player
        direction = (self.player.position - self.position).normalized()
        direction.y = 0  # Keep movement on horizontal plane
        
        # Move towards player
        self.velocity = direction * self.speed
        
        # Look at player
        self.look_at(self.player.position)
    
    def attack_behavior(self):
        if not self.player:
            return
        
        current_time = time.time()
        
        # Check if can attack
        if current_time - self.last_attack_time >= self.attack_cooldown:
            # Face the player
            self.look_at(self.player.position)
            
            # Perform attack
            self.perform_attack()
            self.last_attack_time = current_time
        
        # Stop moving when attacking
        self.velocity = Vec3(0, 0, 0)
    
    def perform_attack(self):
        if not self.player:
            return
        
        # Check if player is still in range
        distance_to_player = distance(self.position, self.player.position)
        if distance_to_player <= self.attack_range:
            # Deal damage to player
            self.player.take_damage(self.damage)
            print(f"Enemy attacked player for {self.damage} damage!")
            
            # Visual effect for attack
            self.attack_effect()
    
    def attack_effect(self):
        # Simple attack animation - scale up and down
        original_scale = self.scale
        self.animate_scale(original_scale * 1.2, duration=0.1)
        self.animate_scale(original_scale, duration=0.1, delay=0.1)
        
        # Change color briefly
        self.color = color.yellow
        self.hit_flash_time = self.hit_flash_duration
    
    def apply_gravity(self):
        # Apply gravity
        self.velocity_y += self.gravity * time.dt
        
        # Apply movement
        self.position += self.velocity * time.dt
        self.position += Vec3(0, self.velocity_y * time.dt, 0)
        
        # Ground check
        ray = raycast(self.position, Vec3(0, -1, 0), distance=5.1)
        if ray.hit:
            if self.velocity_y < 0:
                self.position.y = ray.world_point.y + 2.55  # Half of height
                self.velocity_y = 0
                self.grounded = True
        else:
            self.grounded = False
    
    def take_damage(self, damage):
        self.health -= damage
        
        # Visual feedback
        self.color = color.white
        self.hit_flash_time = self.hit_flash_duration
        
        # Knockback
        if self.player:
            knockback_direction = (self.position - self.player.position).normalized()
            knockback_direction.y = 0
            self.velocity = knockback_direction * 5
        
        if self.health <= 0:
            self.on_death()
    
    def on_death(self):
        print("Enemy defeated!")
        self.drop_loot()
        destroy(self)
    
    def drop_loot(self):
        # Determine loot rarity
        roll = random.random()
        cumulative = 0
        
        for rarity, chance in self.loot_table.items():
            cumulative += chance
            if roll <= cumulative:
                self.create_loot_item(rarity)
                break
    
    def create_loot_item(self, rarity):
        loot_colors = {
            'common': color.gray,
            'rare': color.blue,
            'legendary': color.gold
        }
        
        loot_item = Entity(
            model='sphere',
            color=loot_colors.get(rarity, color.white),
            scale=0.5,
            position=self.position + Vec3(0, 2, 0),
            collider='sphere'
        )
        
        # Add loot component
        loot_item.add_script(LootItem(rarity))
        
        # Bounce animation
        loot_item.animate_y(loot_item.y + 1, duration=0.3, curve=curve.out_bounce)
        
        print(f"Dropped {rarity} loot!")

class LootItem:
    def __init__(self, rarity):
        self.rarity = rarity
        self.collected = False
        
        # Loot stats based on rarity
        self.loot_stats = {
            'common': {'health': 10, 'energy': 5, 'ammo': 15},
            'rare': {'health': 25, 'energy': 15, 'ammo': 30},
            'legendary': {'health': 50, 'energy': 30, 'ammo': 50}
        }
    
    def update(self):
        # Check for player collision
        if hasattr(self.entity, 'position'):
            player_distance = distance(self.entity.position, player.position) if 'player' in globals() else float('inf')
            
            if player_distance < 2 and not self.collected:
                self.collect()
    
    def collect(self):
        if self.collected:
            return
        
        self.collected = True
        
        # Apply loot effects to player
        if 'player' in globals():
            stats = self.loot_stats.get(self.rarity, {})
            
            if 'health' in stats:
                player.heal(stats['health'])
            if 'energy' in stats:
                player.energy = min(player.max_energy, player.energy + stats['energy'])
            if 'ammo' in stats and 'weapon_manager' in globals():
                weapon_manager.add_ammo(stats['ammo'])
            
            print(f"Collected {self.rarity} loot!")
        
        # Remove loot item
        destroy(self.entity)
