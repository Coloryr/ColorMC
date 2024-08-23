#version 330 core

layout (location = 0) in vec3 a_position;
layout (location = 1) in vec2 a_texCoord;
layout (location = 2) in vec3 a_normal;

uniform mat4 model;
uniform mat4 projection;
uniform mat4 view;
uniform mat4 self;

out vec3 normalIn;
out vec2 texIn;
out vec3 fragPosIn;

void main()
{
    texIn = a_texCoord;
	
    mat4 temp = view * model * self;

    fragPosIn = vec3(temp * vec4(a_position, 1.0f));
    normalIn = mat3(transpose(inverse(temp))) * a_normal;

	gl_Position = projection * temp * vec4(a_position, 1.0);
}