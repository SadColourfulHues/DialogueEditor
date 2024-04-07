shader_type canvas_item;
render_mode unshaded, blend_disabled;

//const lowp vec3 mask_compare = vec3(0.25f, 0.45f, 0.22f);

//group_uniforms Pattern;
//uniform lowp sampler2D pattern: source_color;
//uniform lowp vec4 pattern_colour: source_color;
//uniform lowp vec2 pattern_scroll_speed = vec2(0.05, 0.2);
//
//uniform lowp float pattern_scale: hint_range(0.001, 1.0) = 0.25;
//uniform lowp vec2 pattern_spacing = vec2(0.0, 0.0);

group_uniforms Gradient;
uniform lowp float strength: hint_range(0.0, 1.0) = 1.0;
uniform lowp sampler2D gradient_a: source_color;
uniform lowp sampler2D gradient_b: source_color;

group_uniforms Waves;
uniform lowp float wave_size: hint_range(0.01, 1.0) = 0.5;
uniform lowp float wave_speed = 0.5;
uniform lowp float fade_speed = 0.05;

//lowp float get_pattern_mask(vec2 source_uv)
//{
	//// Pattern scroll //
	//lowp vec2 p_uv = source_uv;
	//p_uv.x += pattern_scroll_speed.x * TIME;
	//p_uv.y += pattern_scroll_speed.y * TIME;
//
	//lowp vec2 uv_modifier = pattern_scale + pattern_spacing;
//
	//// Pattern tiling //
	//p_uv = fract(p_uv / uv_modifier) * uv_modifier / pattern_scale;
//
	//lowp vec4 p_colour = texture(pattern, p_uv);
	//lowp float m = (2.0 * dot(p_colour.rgb, mask_compare) - 1.0) * p_colour.a;
//
	//return m;
//}

void fragment()
{
	// Make sure size is consistent across objects
	lowp vec2 suv = SCREEN_UV / (800.0 * SCREEN_PIXEL_SIZE);
	lowp vec2 uv = UV / TEXTURE_PIXEL_SIZE;

	lowp float t = 0.5 + 0.5 * sin(TIME * fade_speed);
	//lowp float m = get_pattern_mask(suv);

	// Sample 'waves' from the gradient textures
	lowp float x = wave_size * uv.y * (dot(uv, suv) + sin(cos(t) + uv.x + (wave_speed * TIME)));
	lowp vec2 uv_a = vec2(mod(suv.y + x, 1.0), 0.0);
	lowp vec2 uv_b = vec2(mod((1.0 - suv.y) - x, 1.0), 0.0);

	// Blend between gradients
	// (TODO: make it look less horrific)
	COLOR = vec4(mix(texture(gradient_a, uv_a).rgb, texture(gradient_b, uv_b).rgb,
				max(0.0, min(1.0, mix(mix(sin(uv.y), cos(uv.x), sin(TIME * fade_speed)),
				mix(cos(uv.y), sin(uv.x), cos(TIME * fade_speed)), t)))), 1.0);

	//// Add pattern on top of the gradient
	//COLOR.rgb = mix(bg, bg + (m * pattern_colour.rgb), pattern_colour.a);
	//COLOR.a = 1.0;
}