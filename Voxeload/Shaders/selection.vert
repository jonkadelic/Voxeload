#version 330 core
layout (location = 0) in vec3 aPosition;

out vec4 fragcolour;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;
uniform vec4 offset;

void main()
{
	gl_Position = projection * view * model * (vec4(aPosition, 1.0) + offset);

	fragcolour = vec4(0.0, 0.0, 0.0, 1.0);
}