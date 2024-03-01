using System;
using System.Text;
using static Prowl.Runtime.GraphicsBackend.DebugLayer.DebugGraphicsDevice;

namespace Prowl.Runtime.GraphicsBackend.DebugLayer;

internal sealed class DebugShader : InternalShader
{
    public InternalShader Shader;
    
    public override bool IsDisposed { get; protected set; }

    public DebugShader(ShaderAttachment[] attachments)
    {
        StringBuilder builder = new StringBuilder();

        foreach (ShaderAttachment attachment in attachments)
            builder.AppendLine($"Attachment:{Environment.NewLine}    Stage: {attachment.Stage}");

        Debug.Log($"Shader info:\n{builder}");

        Shader = Device.CreateShader(attachments);
    }
    
    public override void Dispose()
    {
        Shader.Dispose();
        IsDisposed = Shader.IsDisposed;
        Debug.Log("Shader disposed.");
    }
}