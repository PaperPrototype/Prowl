using static Prowl.Runtime.GraphicsBackend.DebugLayer.DebugGraphicsDevice;

namespace Prowl.Runtime.GraphicsBackend.DebugLayer;

internal sealed class DebugDepthStencilState : DepthStencilState
{
    public DepthStencilState DepthStencilState;

    public override bool IsDisposed { get; protected set; }

    public override DepthStencilStateDescription Description => DepthStencilState.Description;

    public DebugDepthStencilState(DepthStencilStateDescription description)
    {
        Debug.Log($@"Depth stencil state info:
    DepthEnabled: {description.DepthEnabled}
    DepthComparison: {description.DepthComparison}
    DepthMask: {description.DepthMask}
    StencilEnabled: {description.StencilEnabled}
    StencilFrontFace:
        StencilFailOp: {description.StencilFrontFace.StencilFailOp}
        DepthFailOp: {description.StencilFrontFace.DepthFailOp}
        DepthStencilPassOp: {description.StencilFrontFace.DepthStencilPassOp}
        StencilFunc: {description.StencilFrontFace.StencilFunc}
    StencilBackFace:
        StencilFailOp: {description.StencilBackFace.StencilFailOp}
        DepthFailOp: {description.StencilBackFace.DepthFailOp}
        DepthStencilPassOp: {description.StencilBackFace.DepthStencilPassOp}
        StencilFunc: {description.StencilBackFace.StencilFunc}
    StencilReadMask: {description.StencilReadMask}
    StencilWriteMask: {description.StencilWriteMask}");
        
        DepthStencilState = Device.CreateDepthStencilState(description);
    }
    
    public override void Dispose()
    {
        DepthStencilState.Dispose();
        IsDisposed = DepthStencilState.IsDisposed;
        Debug.Log("Depth stencil state disposed.");
    }
}