using Avalonia.OpenGL;
using Live2DCSharpSDK.Framework.Rendering.OpenGL;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Controls.Main;

public class AvaloniaApi : OpenGLApi
{
    private Live2dControl Con;
    private GlInterface GL;
    public override bool IsES2 => true;
    public override bool IsPhoneES2 => false;

    public delegate void Func1(int a, int b);
    public delegate void Func2(int a, int b, int c, int d);
    public delegate void Func3(float a);
    public delegate void Func4(bool a, bool b, bool c, bool d);
    public delegate void Func5(int a);
    public delegate void Func6(int a, int b, int c, int d, int e);
    public unsafe delegate void Func7(int a, bool* b);
    public unsafe delegate void Func8(int a, int* b);
    public delegate void Func9(int a, int b, out int c);
    public delegate bool Func10(int a);
    public delegate void Func11(int a, int b, float c);
    public delegate void Func12(int a, float b, float c, float d, float e);

    public Func1 BlendFunc;
    public Func2 BlendFuncSeparate;
    public Func3 ClearDepthf;
    public Func4 ColorMask;
    public Func1 DetachShader;
    public Func5 Disable;
    public Func5 DisableVertexAttribArray;
    public Func6 FramebufferTexture2D;
    public Func5 FrontFace;
    public Func5 GenerateMipmap;
    public Func7 GetBooleanv;
    public Func8 GetIntegerv;
    public Func9 GetVertexAttribiv;
    public Func10 IsEnabled;
    public Func11 TexParameterf;
    public Func1 Uniform1i;
    public Func12 Uniform4f;
    public Func5 ValidateProgram;

    public AvaloniaApi(Live2dControl con, GlInterface gl)
    {
        Con = con;
        GL = gl;

        BlendFunc = Marshal.GetDelegateForFunctionPointer<Func1>(gl.GetProcAddress("glBlendFunc"));
        BlendFuncSeparate = Marshal.GetDelegateForFunctionPointer<Func2>(gl.GetProcAddress("glBlendFuncSeparate"));
        ClearDepthf = Marshal.GetDelegateForFunctionPointer<Func3>(gl.GetProcAddress("glClearDepthf"));
        ColorMask = Marshal.GetDelegateForFunctionPointer<Func4>(gl.GetProcAddress("glColorMask"));
        DetachShader = Marshal.GetDelegateForFunctionPointer<Func1>(gl.GetProcAddress("glDetachShader"));
        Disable = Marshal.GetDelegateForFunctionPointer<Func5>(gl.GetProcAddress("glDisable"));
        DisableVertexAttribArray = Marshal.GetDelegateForFunctionPointer<Func5>(gl.GetProcAddress("glDisableVertexAttribArray"));
        FramebufferTexture2D = Marshal.GetDelegateForFunctionPointer<Func6>(gl.GetProcAddress("glFramebufferTexture2D"));
        FrontFace = Marshal.GetDelegateForFunctionPointer<Func5>(gl.GetProcAddress("glFrontFace"));
        GenerateMipmap = Marshal.GetDelegateForFunctionPointer<Func5>(gl.GetProcAddress("glGenerateMipmap"));
        GetBooleanv = Marshal.GetDelegateForFunctionPointer<Func7>(gl.GetProcAddress("glGetBooleanv"));
        GetIntegerv = Marshal.GetDelegateForFunctionPointer<Func8>(gl.GetProcAddress("glGetIntegerv"));
        GetVertexAttribiv = Marshal.GetDelegateForFunctionPointer<Func9>(gl.GetProcAddress("glGetVertexAttribiv"));
        IsEnabled = Marshal.GetDelegateForFunctionPointer<Func10>(gl.GetProcAddress("glIsEnabled"));
        TexParameterf = Marshal.GetDelegateForFunctionPointer<Func11>(gl.GetProcAddress("glTexParameterf"));
        Uniform1i = Marshal.GetDelegateForFunctionPointer<Func1>(gl.GetProcAddress("glUniform1i"));
        Uniform4f = Marshal.GetDelegateForFunctionPointer<Func12>(gl.GetProcAddress("glUniform4f"));
        ValidateProgram = Marshal.GetDelegateForFunctionPointer<Func5>(gl.GetProcAddress("glValidateProgram"));
    }

    public override void GetWindowSize(out int w, out int h)
    {
        w = (int)Con.Bounds.Width;
        h = (int)Con.Bounds.Height;
    }

    public override void glActiveTexture(int bit)
    {
        GL.ActiveTexture(bit);
    }

    public override void glAttachShader(int a, int b)
    {
        GL.AttachShader(a, b);
    }

    public override void glBindBuffer(int bit, int index)
    {
        GL.BindBuffer(bit, index);
    }

    public override void glBindFramebuffer(int type, int data)
    {
        GL.BindFramebuffer(type, data);
    }

    public override void glBindTexture(int bit, int index)
    {
        GL.BindTexture(bit, index);
    }

    public override void glBindVertexArrayOES(int data)
    {
        GL.BindVertexArray(data);
    }

    public override void glBlendFunc(int a, int b)
    {
        BlendFunc(a, b);
    }

    public override void glBlendFuncSeparate(int a, int b, int c, int d)
    {
        BlendFuncSeparate(a, b, c, d);
    }

    public override void glClear(int bit)
    {
        GL.Clear(bit);
    }

    public override void glClearColor(float r, float g, float b, float a)
    {
        GL.ClearColor(r, g, b, a);
    }

    public override void glClearDepthf(float data)
    {
        ClearDepthf(data);
    }

    public override void glColorMask(bool a, bool b, bool c, bool d)
    {
        ColorMask(a, b, c, d);
    }

    public override void glCompileShader(int index)
    {
        GL.CompileShader(index);
    }

    public override int glCreateProgram()
    {
        return GL.CreateProgram();
    }

    public override int glCreateShader(int type)
    {
        return GL.CreateShader(type);
    }

    public override void glDeleteFramebuffer(int fb)
    {
        GL.DeleteFramebuffer(fb);
    }

    public override void glDeleteProgram(int index)
    {
        GL.DeleteProgram(index);
    }

    public override void glDeleteShader(int index)
    {
        GL.DeleteShader(index);
    }

    public override void glDeleteTexture(int data)
    {
        GL.DeleteTexture(data);
    }

    public override void glDetachShader(int index, int data)
    {
        DetachShader(index, data);
    }

    public override void glDisable(int bit)
    {
        Disable(bit);
    }

    public override void glDisableVertexAttribArray(int index)
    {
        DisableVertexAttribArray(index);
    }

    public override unsafe void glDrawElements(int type, int count, int type1, ushort* arry)
    {
        GL.DrawElements(type, count, type1, new IntPtr(arry));
    }

    public override void glEnable(int bit)
    {
        GL.Enable(bit);
    }

    public override void glEnableVertexAttribArray(int index)
    {
        GL.EnableVertexAttribArray(index);
    }

    public override void glFramebufferTexture2D(int a, int b, int c, int buff, int data)
    {
        FramebufferTexture2D(a, b, c, buff, data);
    }

    public override void glFrontFace(int data)
    {
        FrontFace(data);
    }

    public override void glGenerateMipmap(int a)
    {
        GenerateMipmap(a);
    }

    public override int glGenFramebuffer()
    {
        return GL.GenFramebuffer();
    }

    public override int glGenTexture()
    {
       return  GL.GenTexture();
    }

    public override int glGetAttribLocation(int index, string attr)
    {
        return GL.GetAttribLocationString(index, attr);
    }

    public override unsafe void glGetBooleanv(int bit, bool[] data)
    {
        fixed (bool* ptr = data)
            GetBooleanv(bit, ptr);
    }

    public override int glGetError()
    {
        return GL.GetError();
    }

    public override void glGetIntegerv(int bit, out int data)
    {
        GL.GetIntegerv(bit, out data);
    }

    public override unsafe void glGetIntegerv(int bit, int[] data)
    {
        fixed (int* ptr = data)
            GetIntegerv(bit, ptr);
    }

    public override unsafe void glGetProgramInfoLog(int index, out string log)
    {
        int logLength;
        GL.GetProgramiv(index, GL_INFO_LOG_LENGTH, &logLength);
        var logData = new byte[logLength];
        int len;
        fixed (void* ptr = logData)
            GL.GetProgramInfoLog(index, logLength, out len, ptr);
        log = Encoding.UTF8.GetString(logData, 0, len);
    }

    public override unsafe void glGetProgramiv(int index, int type, int* length)
    {
        GL.GetProgramiv(index, type, length);
    }

    public override unsafe void glGetShaderInfoLog(int index, out string log)
    {
        int logLength;
        GL.GetShaderiv(index, GL_INFO_LOG_LENGTH, &logLength);
        var logData = new byte[logLength];
        int len;
        fixed (void* ptr = logData)
            GL.GetShaderInfoLog(index, logLength, out len, ptr);
        log = Encoding.UTF8.GetString(logData, 0, len);
    }

    public override unsafe void glGetShaderiv(int index, int type, int* length)
    {
        GL.GetShaderiv(index, type, length);
    }

    public override int glGetUniformLocation(int index, string uni)
    {
        return GL.GetUniformLocationString(index, uni);
    }

    public override void glGetVertexAttribiv(int index, int bit, out int data)
    {
        GetVertexAttribiv(index, bit, out data);
    }

    public override bool glIsEnabled(int bit)
    {
        return IsEnabled(bit);
    }

    public override void glLinkProgram(int index)
    {
        GL.LinkProgram(index);
    }

    public override void glShaderSource(int a, string source)
    {
        GL.ShaderSourceString(a, source);
    }

    public override void glTexImage2D(int type, int a, int type1, int w, int h, int size, int type2, int type3, nint data)
    {
        GL.TexImage2D(type, a, type1, w, h, size, type2, type3, data);
    }

    public override void glTexParameterf(int type, int type1, float value)
    {
        TexParameterf(type, type1, value);
    }

    public override void glTexParameteri(int a, int b, int c)
    {
        GL.TexParameteri(a, b, c);
    }

    public override void glUniform1i(int index, int data)
    {
        Uniform1i(index, data);
    }

    public override void glUniform4f(int index, float a, float b, float c, float d)
    {
        Uniform4f(index, a, b, c, d);
    }

    public override unsafe void glUniformMatrix4fv(int index, int length, bool b, float[] data)
    {
        fixed (float* ptr = data)
            GL.UniformMatrix4fv(index, length, b, ptr);
    }

    public override void glUseProgram(int index)
    {
        GL.UseProgram(index);
    }

    public override void glValidateProgram(int index)
    {
        ValidateProgram(index);
    }

    public override unsafe void glVertexAttribPointer(int index, int length, int type, bool b, int size, float* arr)
    {
        GL.VertexAttribPointer(index, length, type, b ? 1: 0, size, new IntPtr(arr));
    }

    public override void glViewport(int x, int y, int w, int h)
    {
        GL.Viewport(x, y, w, h);
    }
}
