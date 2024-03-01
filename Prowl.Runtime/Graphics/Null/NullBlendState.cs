using System;

namespace Prowl.Runtime.GraphicsBackend.Null;

internal sealed class NullBlendState : BlendState
{
    public override bool IsDisposed { get; protected set; }
    public override BlendStateDescription Description { get; }

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
