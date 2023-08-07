using Avalonia.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Controls.Skin;

public static class Shader
{
    private const string VertexShaderSource =
@"attribute vec3 a_position;
attribute vec2 a_texCoord;
attribute vec3 a_normal;

uniform mat4 model;
uniform mat4 projection;
uniform mat4 view;
uniform mat4 self;

varying vec3 normalIn;
varying vec2 texIn;
varying vec3 fragPosIn;

void main()
{
    texIn = a_texCoord;

    mat4 temp = view * model * self;

    fragPosIn = vec3(model * vec4(a_position, 1.0));
    normalIn = normalize(vec3(model * vec4(a_normal, 1.0)));
    gl_Position = projection * temp * vec4(a_position, 1.0);
}
";

    private const string FragmentShaderSource =
@"uniform sampler2D texture0;
varying vec3 fragPosIn;
varying vec3 normalIn;
varying vec2 texIn;
//DECLAREGLFRAG
void main()
{
    vec3 lightColor = vec3(1.0, 1.0, 1.0);
    float ambientStrength = 0.15;
    vec3 lightPos = vec3(0, 1, 5);
    
    vec3 ambient = ambientStrength * lightColor;

    vec3 norm = normalize(normalIn);
    vec3 lightDir = normalize(lightPos - fragPosIn);
    float diff = max(dot(norm, lightDir), 0.0);
    vec3 diffuse = diff * lightColor;

    vec3 result = (ambient + diffuse);
    gl_FragColor = texture2D(texture0, texIn) * vec4(result, 1.0);
    //gl_FragColor = texture2D(texture0, texIn);
}
";

    private static string GetShader(GlVersion gl, bool fragment, string shader)
    {
        var version = (gl.Type == GlProfileType.OpenGL ?
            RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? 150 : 120 :
            100);
        var data = "#version " + version + "\n";
        if (gl.Type == GlProfileType.OpenGLES)
            data += "precision mediump float;\n";
        if (version >= 150)
        {
            shader = shader.Replace("attribute", "in");
            if (fragment)
                shader = shader
                    .Replace("varying", "in")
                    .Replace("//DECLAREGLFRAG", "out vec4 outFragColor;")
                    .Replace("gl_FragColor", "outFragColor")
                    .Replace("texture2D", "texture");
            else
                shader = shader.Replace("varying", "out");
        }

        data += shader;

        return data;
    }

    public static string VertexShader(GlVersion gl, bool fragment)
    {
        return GetShader(gl, fragment, fragment ? FragmentShaderSource : VertexShaderSource);
    }
}
