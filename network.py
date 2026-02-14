import socket
import threading
import json
import time
from typing import Dict, List, Any

class NetworkManager:
    def __init__(self, host='localhost', port=5555):
        self.host = host
        self.port = port
        self.socket = None
        self.clients = {}
        self.is_server = False
        self.is_connected = False
        self.player_id = None
        self.callbacks = {}
        
    def start_server(self):
        """Start the server"""
        self.is_server = True
        self.socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        self.socket.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
        self.socket.bind((self.host, self.port))
        self.socket.listen(5)
        
        print(f"Server started on {self.host}:{self.port}")
        
        # Start accepting connections
        accept_thread = threading.Thread(target=self.accept_connections)
        accept_thread.daemon = True
        accept_thread.start()
        
        self.is_connected = True
        self.player_id = "server"
    
    def connect_to_server(self, server_host='localhost', server_port=5555):
        """Connect to a server"""
        self.is_server = False
        try:
            self.socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
            self.socket.connect((server_host, server_port))
            
            print(f"Connected to server at {server_host}:{server_port}")
            
            # Start receiving messages
            receive_thread = threading.Thread(target=self.receive_messages)
            receive_thread.daemon = True
            receive_thread.start()
            
            self.is_connected = True
            self.player_id = f"client_{int(time.time())}"
            
            # Send initial connection message
            self.send_message({
                'type': 'connect',
                'player_id': self.player_id
            })
            
        except Exception as e:
            print(f"Failed to connect to server: {e}")
            self.is_connected = False
    
    def accept_connections(self):
        """Accept incoming client connections"""
        while self.is_connected:
            try:
                client_socket, address = self.socket.accept()
                client_id = f"client_{int(time.time())}_{len(self.clients)}"
                
                self.clients[client_id] = client_socket
                print(f"Client connected: {client_id} from {address}")
                
                # Start handling client messages
                client_thread = threading.Thread(
                    target=self.handle_client,
                    args=(client_socket, client_id)
                )
                client_thread.daemon = True
                client_thread.start()
                
                # Notify other clients about new player
                self.broadcast_message({
                    'type': 'player_joined',
                    'player_id': client_id
                }, exclude_client=client_id)
                
            except Exception as e:
                if self.is_connected:
                    print(f"Error accepting connection: {e}")
    
    def handle_client(self, client_socket, client_id):
        """Handle messages from a specific client"""
        while self.is_connected:
            try:
                data = client_socket.recv(4096)
                if not data:
                    break
                
                message = json.loads(data.decode('utf-8'))
                message['sender_id'] = client_id
                
                # Process the message
                self.process_message(message)
                
                # Broadcast to other clients
                self.broadcast_message(message, exclude_client=client_id)
                
            except Exception as e:
                print(f"Error handling client {client_id}: {e}")
                break
        
        # Clean up disconnected client
        if client_id in self.clients:
            del self.clients[client_id]
            print(f"Client disconnected: {client_id}")
            
            # Notify other clients
            self.broadcast_message({
                'type': 'player_left',
                'player_id': client_id
            })
    
    def receive_messages(self):
        """Receive messages from server"""
        while self.is_connected:
            try:
                data = self.socket.recv(4096)
                if not data:
                    break
                
                message = json.loads(data.decode('utf-8'))
                self.process_message(message)
                
            except Exception as e:
                print(f"Error receiving message: {e}")
                break
        
        self.is_connected = False
    
    def process_message(self, message):
        """Process received message"""
        message_type = message.get('type')
        
        if message_type in self.callbacks:
            self.callbacks[message_type](message)
        else:
            print(f"Received message: {message}")
    
    def send_message(self, message):
        """Send message to server or broadcast to all clients"""
        if not self.is_connected:
            return
        
        try:
            data = json.dumps(message).encode('utf-8')
            
            if self.is_server:
                # Broadcast to all clients
                self.broadcast_message(message)
            else:
                # Send to server
                self.socket.send(data)
                
        except Exception as e:
            print(f"Error sending message: {e}")
    
    def broadcast_message(self, message, exclude_client=None):
        """Broadcast message to all clients (server only)"""
        if not self.is_server:
            return
        
        data = json.dumps(message).encode('utf-8')
        
        disconnected_clients = []
        for client_id, client_socket in self.clients.items():
            if client_id == exclude_client:
                continue
                
            try:
                client_socket.send(data)
            except Exception as e:
                print(f"Error broadcasting to {client_id}: {e}")
                disconnected_clients.append(client_id)
        
        # Remove disconnected clients
        for client_id in disconnected_clients:
            if client_id in self.clients:
                del self.clients[client_id]
    
    def register_callback(self, message_type, callback):
        """Register callback for specific message types"""
        self.callbacks[message_type] = callback
    
    def send_player_update(self, position, rotation, health):
        """Send player state update"""
        self.send_message({
            'type': 'player_update',
            'player_id': self.player_id,
            'position': list(position),
            'rotation': list(rotation),
            'health': health
        })
    
    def send_shoot_event(self, direction, hit_position=None):
        """Send shooting event"""
        message = {
            'type': 'shoot',
            'player_id': self.player_id,
            'direction': list(direction)
        }
        
        if hit_position:
            message['hit_position'] = list(hit_position)
        
        self.send_message(message)
    
    def send_enemy_update(self, enemy_id, position, health):
        """Send enemy state update"""
        self.send_message({
            'type': 'enemy_update',
            'enemy_id': enemy_id,
            'position': list(position),
            'health': health
        })
    
    def disconnect(self):
        """Disconnect from server or stop server"""
        self.is_connected = False
        
        if self.socket:
            try:
                if self.is_server:
                    self.socket.close()
                else:
                    self.send_message({
                        'type': 'disconnect',
                        'player_id': self.player_id
                    })
                    self.socket.close()
            except:
                pass
        
        if self.is_server:
            for client_socket in self.clients.values():
                try:
                    client_socket.close()
                except:
                    pass
            self.clients.clear()

# Example usage and message handlers
class GameNetwork:
    def __init__(self):
        self.network = NetworkManager()
        self.setup_callbacks()
        self.players = {}
        self.enemies = {}
    
    def setup_callbacks(self):
        """Setup network message callbacks"""
        self.network.register_callback('connect', self.on_player_connect)
        self.network.register_callback('disconnect', self.on_player_disconnect)
        self.network.register_callback('player_update', self.on_player_update)
        self.network.register_callback('shoot', self.on_shoot)
        self.network.register_callback('enemy_update', self.on_enemy_update)
    
    def on_player_connect(self, message):
        """Handle new player connection"""
        player_id = message.get('player_id')
        print(f"Player {player_id} connected")
        # Add player to game world
    
    def on_player_disconnect(self, message):
        """Handle player disconnection"""
        player_id = message.get('player_id')
        print(f"Player {player_id} disconnected")
        # Remove player from game world
    
    def on_player_update(self, message):
        """Handle player state update"""
        player_id = message.get('player_id')
        position = message.get('position')
        rotation = message.get('rotation')
        health = message.get('health')
        
        # Update player in game world
        self.players[player_id] = {
            'position': position,
            'rotation': rotation,
            'health': health
        }
    
    def on_shoot(self, message):
        """Handle shooting event"""
        player_id = message.get('player_id')
        direction = message.get('direction')
        hit_position = message.get('hit_position')
        
        # Create visual effects for shooting
        print(f"Player {player_id} shot in direction {direction}")
    
    def on_enemy_update(self, message):
        """Handle enemy state update"""
        enemy_id = message.get('enemy_id')
        position = message.get('position')
        health = message.get('health')
        
        # Update enemy in game world
        self.enemies[enemy_id] = {
            'position': position,
            'health': health
        }
    
    def start_server(self):
        """Start game server"""
        self.network.start_server()
    
    def connect_to_server(self, host='localhost', port=5555):
        """Connect to game server"""
        self.network.connect_to_server(host, port)
