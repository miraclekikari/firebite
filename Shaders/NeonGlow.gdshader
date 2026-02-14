shader_type spatial;
render_mode unshaded, cull_disabled;

uniform vec4 glow_color : source_color = vec4(0.0, 0.8, 1.0, 1.0);
uniform float glow_intensity : hint_range(0.0, 10.0) = 2.0;
uniform float pulse_speed : hint_range(0.0, 5.0) = 1.0;
uniform float pulse_amount : hint_range(0.0, 1.0) = 0.3;

varying vec3 world_position;

void vertex() {
    world_position = VERTEX;
}

void fragment() {
    // Pulsation du néon
    float pulse = sin(TIME * pulse_speed) * pulse_amount + 1.0;
    
    // Effet de gradient vertical
    float gradient = clamp(world_position.y / 15.0, 0.0, 1.0);
    
    // Couleur de base avec pulsation
    vec3 base_color = glow_color.rgb * glow_intensity * pulse;
    
    // Effet de bordure lumineuse
    float edge_factor = abs(VERTEX.x) + abs(VERTEX.z);
    float edge_glow = 1.0 - smoothstep(0.0, 1.5, edge_factor);
    
    // Couleur finale
    vec3 final_color = base_color * (0.7 + gradient * 0.3) + edge_glow * base_color * 0.5;
    
    // Alpha pour la transparence
    float alpha = glow_color.a * (0.8 + edge_glow * 0.2);
    
    ALBEDO = final_color;
    ALPHA = alpha;
    EMISSION = final_color * 2.0;
}
