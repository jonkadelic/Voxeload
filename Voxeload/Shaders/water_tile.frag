#version 330 core
in vec2 texCoord;
flat in int texFace;
in float brightness;

out vec4 colour;

uniform sampler2D texture0;

void main()
{
	vec4 c = texture(texture0, texCoord);
	if (c.a == 0.0) discard;

	float mult = 1.0f;

	int f = texFace & 0xFF;

	if (f == 0x01 || f == 0x02) mult = 0.85f;
	else if (f == 0x04) mult = 0.55f;
	else if (f == 0x08) mult = 1.0f;
	else if (f == 0x10 || f == 0x20) mult = 0.7f;

	mult *= brightness;

	colour = c * vec4(mult, mult, mult, 1.0f);
}