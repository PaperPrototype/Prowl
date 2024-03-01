using static Prowl.Runtime.GraphicsBackend.DebugLayer.DebugGraphicsDevice;

namespace Prowl.Runtime.GraphicsBackend.DebugLayer;

internal sealed class DebugBlendState : BlendState
{
    public BlendState BlendState;
    
    public override bool IsDisposed { get; protected set; }

    public override BlendStateDescription Description => BlendState.Description;

    public DebugBlendState(BlendStateDescription description)
    {
        Debug.Log($@"Blend state info:
    Enabled: {description.Enabled}
    Source: {description.Source}
    Destination: {description.Destination}
    BlendOperation: {description.BlendOperation}
    SourceAlpha: {description.SourceAlpha}
    DestinationAlpha: {description.DestinationAlpha}
    AlphaBlendOperation: {description.AlphaBlendOperation}
    ColorWriteMask: {description.ColorWriteMask}");
        
        // TODO: There are a bunch of unsupported blending combinations. Find out what they are and add checks.
        BlendState = Device.CreateBlendState(description);
    }
    
    public override void Dispose()
    {
        BlendState.Dispose();
        IsDisposed = BlendState.IsDisposed;
        Debug.Log("Blend state disposed.");
    }
}