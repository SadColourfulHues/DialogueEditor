shader_type canvas_item;

uniform lowp sampler2D background: hint_screen_texture;

uniform lowp float blur_level: hint_range(0, 4) = 4;
uniform lowp float blur_mix: hint_range(0.0f, 1.0f) = 0.5f;

void fragment()
{
	lowp vec4 blurred = textureLod(background, SCREEN_UV, blur_level);
	COLOR = mix(COLOR, blurred, blur_mix);
}
