#version 330 core
in vec2 texCoords;

out vec4 colour;

uniform sampler2D screenTexture;

void main()
{
	colour = texture(screenTexture, texCoords);
}