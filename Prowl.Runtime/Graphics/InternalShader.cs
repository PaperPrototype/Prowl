using System;

namespace Prowl.Runtime.GraphicsBackend;

/// <summary>
/// A shader is a small program that is executed on the GPU. They are often used to transform vertices and draw pixels
/// (fragments) to the screen.
/// </summary>
public abstract class InternalShader : IDisposable
{
    /// <summary>
    /// Will return <see langword="true" /> when this <see cref="InternalShader"/> has been disposed.
    /// </summary>
    public abstract bool IsDisposed { get; protected set; }

    /// <summary>
    /// Dispose of this <see cref="InternalShader"/>.
    /// </summary>
    public abstract void Dispose();
}