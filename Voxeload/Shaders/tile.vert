#version 330 core
layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec4 colour;

out vec4 fragcolour;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;
uniform vec4 offset;

void main()
{
	gl_Position = projection * view * model * (vec4(aPosition, 1.0) + offset);

	fragcolour = colour;
}