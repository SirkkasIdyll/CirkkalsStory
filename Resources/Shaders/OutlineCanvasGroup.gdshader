shader_type canvas_item;

uniform vec4 line_colour: source_color = vec4(1.0);
uniform int line_thickness: hint_range(0, 10) = 1;
uniform sampler2D screen_texture : hint_screen_texture, repeat_disable, filter_nearest;
render_mode unshaded;

void vertex() {
	// Called for every vertex the material is visible on.
}

void fragment() {
	vec2 size = SCREEN_PIXEL_SIZE * float(line_thickness);
	float line_alpha = 0.0;
	for (float i = -size.x; i <= size.x; i += SCREEN_PIXEL_SIZE.x) {
		for (float j = -size.y; j <= size.y; j += SCREEN_PIXEL_SIZE.y) {
			line_alpha += texture(screen_texture, SCREEN_UV + vec2(i, j)).a;
		}
	}
	vec4 colour = texture(screen_texture, SCREEN_UV);
	vec4 outline = vec4(line_colour.x, line_colour.y, line_colour.z,  min(line_alpha, line_colour.w));
	COLOR *= mix(outline, colour, colour.a);
}

//void light() {
//	// Called for every pixel for every light affecting the CanvasItem.
//	// Uncomment to replace the default light processing function with this one.
//}