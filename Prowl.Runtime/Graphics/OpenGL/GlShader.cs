using Silk.NET.OpenGL;
using System;
using static Prowl.Runtime.GraphicsBackend.OpenGL.GlGraphicsDevice;

namespace Prowl.Runtime.GraphicsBackend.OpenGL;

internal sealed class GlShader : InternalShader
{
    public override bool IsDisposed { get; protected set; }

    public uint Handle;

    public unsafe GlShader(ShaderAttachment[] attachments)
    {
        Handle = Gl.CreateProgram();

        uint* shaders = stackalloc uint[attachments.Length];
        
        for (int i = 0; i < attachments.Length; i++)
        {
            ref ShaderAttachment attachment = ref attachments[i];
            
            ShaderType type = attachment.Stage switch
            {
                ShaderStage.Vertex => ShaderType.VertexShader,
                ShaderStage.Fragment => ShaderType.FragmentShader,
                ShaderStage.Geometry => ShaderType.GeometryShader,
                ShaderStage.Compute => ShaderType.ComputeShader,
                _ => throw new ArgumentOutOfRangeException()
            };

            uint shader = Gl.CreateShader(type);
            shaders[i] = shader;

            Gl.ShaderSource(shader, attachment.Source);
            
            Gl.CompileShader(shader);

            Gl.GetShader(shader, ShaderParameterName.CompileStatus, out int compStatus);
            if (compStatus != (int) GLEnum.True)
                throw new GraphicsException($"OpenGL: Failed to compile {attachment.Stage} shader! " + Gl.GetShaderInfoLog(shader));
            
            Gl.AttachShader(Handle, shader);
        }
        
        Gl.LinkProgram(Handle);

        Gl.GetProgram(Handle, ProgramPropertyARB.LinkStatus, out int linkStatus);
        if (linkStatus != (int) GLEnum.True)
            throw new GraphicsException("OpenGL: Failed to link program! " + Gl.GetProgramInfoLog(Handle));

        for (int i = 0; i < attachments.Length; i++)
        {
            uint shader = shaders[i];
            
            Gl.DetachShader(Handle, shader);
            Gl.DeleteShader(shader);
        }
    }

    public override void Dispose()
    {
        Gl.DeleteProgram(Handle);
    }
}