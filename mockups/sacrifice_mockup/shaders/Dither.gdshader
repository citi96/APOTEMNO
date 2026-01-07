shader_type canvas_item;

uniform sampler2D screen_texture : hint_screen_texture, filter_nearest;
uniform float sanity : hint_range(0.0, 1.0) = 1.0;

// 4x4 Bayer Matrix for ordered dithering
const int bayer4x4[16] = int[](
    0,  8,  2, 10,
    12, 4, 14,  6,
    3, 11,  1,  9,
    15, 7, 13,  5
);

void fragment() {
    // 1. Sample the screen color
    vec4 raw_color = texture(screen_texture, SCREEN_UV);
    
    // 2. Convert to Greyscale (Luma)
    float luma = dot(raw_color.rgb, vec3(0.299, 0.587, 0.114));
    
    // 3. Apply Sanity Degradation
    // As sanity drops, image gets darker/harder to see
    // We actively reduce the luma based on sanity loss
    float degradation = 1.0 - sanity;
    
    // Option A: Darkness (Luma reduced)
    luma = luma - (degradation * 0.4); 
    
    // Option B: Noise/Static (Optional, maybe too chaotic?)
    // keeping it simple: just dark "void" creeping in.
    
    // 4. Dithering Lookup
    // Map pixel coordinates to 4x4 grid
    int x = int(FRAGCOORD.x) % 4;
    int y = int(FRAGCOORD.y) % 4;
    int index = y * 4 + x;
    
    // Normalize bayer value (0..15 -> 0.0..1.0)
    float threshold = float(bayer4x4[index]) / 16.0;
    
    // 5. 1-Bit Decision
    // If sanity is low, we can also mess with the threshold to add noise
    // But standard dither:
    vec3 final_color;
    if (luma > threshold) {
        final_color = vec3(1.0); // White
    } else {
        final_color = vec3(0.0); // Black
    }
    
    COLOR = vec4(final_color, 1.0);
}
