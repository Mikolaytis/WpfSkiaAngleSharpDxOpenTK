#version 300 es

precision mediump float;
in vec2 tex_coord;
out vec4 frag_color;

uniform sampler2D tex0;

void main()
{
	vec4 color = texture(tex0, tex_coord);
	if (color.x == 0.0 && color.y == 0.0 && color.z == 0.0)
		color.xy = tex_coord;
    frag_color = vec4(color.xyz, 1);
}
