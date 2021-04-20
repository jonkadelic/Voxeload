#version 330 core
in vec2 texCoords;

out vec4 colour;

uniform sampler2D screenTexture;

void main()
{
	vec4 c = texture(screenTexture, texCoords);
	float scale = 8;
	float r = int(c.r * scale) / scale;
	float g = int(c.g * scale) / scale;
	float b = int(c.b * scale) / scale;
	colour = vec4(r, g, b, c.a);
}