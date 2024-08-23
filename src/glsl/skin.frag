#version 330 core

uniform sampler2D texture0;
uniform vec3 lightColor;

in vec3 fragPosIn;
in vec3 normalIn;
in vec2 texIn;

out vec4 FragColor;

void main()
{
    float ambientStrength = 0.1f;
    vec3 ambient = ambientStrength * lightColor;
 
    vec3 norm = normalize(normalIn);
    vec3 lightDir = normalize(-fragPosIn);
    float diff = max(dot(norm, lightDir), 0.0f);
    vec3 diffuse = diff * lightColor;
 
    vec3 result = (ambient + diffuse);

    FragColor = texture(texture0, texIn) * vec4(result, 1.0f);
}