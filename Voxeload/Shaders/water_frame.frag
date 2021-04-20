#version 330 core
in vec2 texCoords;

out vec4 colour;

uniform sampler2D screenTexture;
uniform sampler2D screenDepthTexture;
uniform sampler2D worldDepthTexture;

void main()
{
	vec4 a = texture(screenDepthTexture, texCoords);
	vec4 b = texture(worldDepthTexture, texCoords);
	if (a.r > b.r) discard;
	colour = texture(screenTexture, texCoords);
	float cscale = 1.0f;
	float ascale = (1.0f + (b.r * 128.0f - 127.0f));
	if (ascale < 1.0f) ascale = 1.0f;
	if (ascale > 1.35f) ascale = 1.35f;
	colour = vec4(colour.r * cscale, colour.g * cscale, colour.b * cscale, colour.a * ascale);
}