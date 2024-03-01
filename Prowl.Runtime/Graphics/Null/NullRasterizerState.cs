using System;

namespace Prowl.Runtime.GraphicsBackend.Null;

internal sealed class NullRasterizerState : RasterizerState
{
    public override bool IsDisposed { get; protected set; }
    public override RasterizerStateDescription Description { get; }

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
