using System;
using System.Drawing;

namespace Prowl.Runtime.GraphicsBackend;

/// <summary>
/// A texture is a data store that can be sampled from in a <see cref="InternalShader"/>. This includes preloaded texture data,
/// or a <see cref="Framebuffer"/>.
/// </summary>
public abstract class InternalTexture : IDisposable
{
    /// <summary>
    /// Will return <see langword="true" /> when this <see cref="InternalTexture"/> has been disposed.
    /// </summary>
    public abstract bool IsDisposed { get; protected set; }

    /// <summary>
    /// The <see cref="TextureDescription"/> of this <see cref="InternalTexture"/>.
    /// </summary>
    public abstract TextureDescription Description { get; set; }

    /// <summary>
    /// Dispose of this <see cref="InternalTexture"/>.
    /// </summary>
    public abstract void Dispose();
}