using static Prowl.Runtime.GraphicsBackend.DebugLayer.DebugGraphicsDevice;

namespace Prowl.Runtime.GraphicsBackend.DebugLayer;

internal sealed class DebugRasterizerState : RasterizerState
{
    public RasterizerState RasterizerState;
    
    public override bool IsDisposed { get; protected set; }

    public override RasterizerStateDescription Description => RasterizerState.Description;

    public DebugRasterizerState(RasterizerStateDescription description)
    {
        Debug.Log($@"Rasterizer state info:
    CullDirection: {description.CullDirection}
    CullFace: {description.CullFace}
    FillMode: {description.FillMode}
    ScissorTestEnabled: {description.ScissorTest}");
        
        RasterizerState = Device.CreateRasterizerState(description);
    }
    
    public override void Dispose()
    {
        RasterizerState.Dispose();
        IsDisposed = RasterizerState.IsDisposed;
        Debug.Log("Rasterizer state disposed.");
    }
}