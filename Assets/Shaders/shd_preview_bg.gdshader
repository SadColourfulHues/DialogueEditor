shader_type canvas_item;
render_mode unshaded, blend_disabled;

group_uniforms Gradient;
uniform lowp float strength: hint_range(0.0, 1.0) = 1.0;
uniform lowp sampler2D gradient_a: source_color;
uniform lowp sampler2D gradient_b: source_color;

group_uniforms Waves;
uniform lowp float wave_size: hint_range(0.01, 1.0) = 0.5;
uniform lowp float wave_speed = 0.5;
uniform lowp float fade_speed = 0.05;

void fragment()
{
	// Make sure size is consistent across objects
	lowp vec2 suv = SCREEN_UV / (800.0 * SCREEN_PIXEL_SIZE);
	lowp vec2 uv = UV / TEXTURE_PIXEL_SIZE;

	lowp float t = 0.5 + 0.5 * sin(TIME * fade_speed);

	// Sample 'waves' from the gradient textures
	lowp float x = wave_size * uv.y * (dot(uv, suv) + sin(cos(t) + uv.x + (wave_speed * TIME)));
	lowp vec2 uv_a = vec2(mod(suv.y + x, 1.0), 0.0);
	lowp vec2 uv_b = vec2(mod((1.0 - suv.y) - x, 1.0), 0.0);

	// Blend between gradients
	// (TODO: make it look less horrific)
	COLOR = vec4(mix(texture(gradient_a, uv_a).rgb, texture(gradient_b, uv_b).rgb,
				max(0.0, min(1.0, mix(mix(sin(uv.y), cos(uv.x), sin(TIME * fade_speed)),
				mix(cos(uv.y), sin(uv.x), cos(TIME * fade_speed)), t)))), 1.0);
}