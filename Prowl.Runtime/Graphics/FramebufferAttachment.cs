namespace Prowl.Runtime.GraphicsBackend;

/// <summary>
/// A texture attachment for a <see cref="Framebuffer"/>
/// </summary>
public struct FramebufferAttachment
{
    /// <summary>
    /// The <see cref="Prowl.Runtime.GraphicsBackend.InternalTexture"/> to attach.
    /// </summary>
    public InternalTexture Texture;

    /// <summary>
    /// Create a new framebuffer attachment.
    /// </summary>
    /// <param name="texture">The <see cref="Prowl.Runtime.GraphicsBackend.InternalTexture"/> to attach.</param>
    public FramebufferAttachment(InternalTexture texture)
    {
        Texture = texture;
    }
}