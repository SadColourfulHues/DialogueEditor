shader_type canvas_item;

uniform lowp sampler2D gradient_a;
uniform lowp sampler2D gradient_b;
uniform lowp float scroll_speed = 0.1;
uniform lowp float fade_speed = 0.05;

void fragment()
{
	lowp float x = sin(scroll_speed * TIME) * UV.x;
	lowp vec2 uv_a = vec2(UV.y + x, 0.0);
	lowp vec2 uv_b = vec2((1.0 - UV.y) - x, 0.0);

	lowp vec4 colour_a = texture(gradient_a, uv_a);
	lowp vec4 colour_b = texture(gradient_b, uv_b);

	COLOR = mix(colour_a, colour_b, 0.5 + 0.5 * sin(fade_speed * TIME));
}