shader_type spatial;

// Base material properties
uniform vec4 albedo : source_color = vec4(1.0);
uniform float metallic : hint_range(0.0, 1.0) = 0.1;
uniform float roughness : hint_range(0.0, 1.0) = 0.7;
uniform vec4 emission_color : source_color = vec4(1.0, 0.01, 0.0, 1.0);
uniform float emission_strength : hint_range(0.0, 500.0) = 200.0;
uniform sampler2D texture_albedo : source_color;

// Vertex offset parameters
uniform float displacement_amount = 0.0;
uniform float direction_randomness : hint_range(0.0, 1.0) = 0.5;
uniform float vertex_time_scale : hint_range(0.0, 2.0) = 0.1;

// Dissolve parameters
uniform float dissolve_amount : hint_range(0.0, 1.0) = 0.0;
uniform vec4 dissolve_color : source_color = vec4(1.0, 0.3, 0.0, 1.0);
uniform float edge_width : hint_range(0.0, 0.5) = 0.1;
uniform float edge_intensity : hint_range(0.0, 10.0) = 2.0;
uniform float dissolve_time_scale : hint_range(0.0, 2.0) = 0.15;

// Noise textures
uniform sampler2D noise_texture;
uniform sampler2D displacement_texture; // Can be the same as noise_texture

void vertex() {
    // Only displace vertices if displacement_amount is greater than 0
    if (displacement_amount > 0.0) {
        // Create animated UVs by offsetting with TIME
        vec2 animated_uvs = UV;
        animated_uvs.x += TIME * vertex_time_scale; // Use the time scale uniform
        animated_uvs.y += TIME * (vertex_time_scale * 0.5); // Half speed for Y for variation
        
        // Get displacement vector from texture with animated UVs
        vec4 displacement_sample = texture(displacement_texture, animated_uvs);
        vec3 direction = NORMAL;
        
        // Add some randomness to the direction if needed
        if (direction_randomness > 0.0) {
            vec3 random_dir = (displacement_sample.rgb * 2.0 - 1.0);
            direction = mix(direction, random_dir, direction_randomness);
            direction = normalize(direction);
        }
        
        // Apply displacement
        VERTEX += direction * displacement_amount * displacement_sample.r;
    }
}

void fragment() {
    // Apply base material properties
    ALBEDO = albedo.rgb;
    METALLIC = metallic;
    ROUGHNESS = roughness;
    
    // Apply constant emission
    EMISSION = vec3(1.0, 0.01, 0.0) * 200.0;
    
    // Apply dissolve effect if enabled
    if (dissolve_amount > 0.0) {
        // Create animated UVs for the dissolve effect
        vec2 dissolve_uvs = UV;
        dissolve_uvs.x += TIME * dissolve_time_scale; // Use the dissolve time scale uniform
        dissolve_uvs.y += TIME * (dissolve_time_scale * 0.5);
        
        // Get noise value for dissolve effect with animated UVs
        float noise_value = texture(noise_texture, dissolve_uvs).r;
        
        // Make fragments with noise value below threshold transparent
        if (noise_value < dissolve_amount) {
            ALPHA = 0.0;
        }
    }
}