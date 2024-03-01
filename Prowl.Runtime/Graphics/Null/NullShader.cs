using System;

namespace Prowl.Runtime.GraphicsBackend.Null;

internal sealed class NullShader : InternalShader
{
    public override bool IsDisposed { get; protected set; }

    public override void Dispose()
    {
        if (IsDisposed)
        {
            return;
        }

        IsDisposed = true;

        GC.SuppressFinalize(this);
    }
}
