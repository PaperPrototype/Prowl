namespace Prowl.Runtime.GraphicsBackend;

/// <summary>
/// Attach shader code at the given <see cref="Stage"/> to a new <see cref="InternalShader"/>.
/// </summary>
public struct ShaderAttachment
{
    /// <summary>
    /// The stage of this shader attachment.
    /// </summary>
    public ShaderStage Stage;
    
    /// <summary>
    /// The source code of this shader attachment.
    /// </summary>
    public string Source;

    /// <summary>
    /// Create a new shader attachment.
    /// </summary>
    /// <param name="stage">The stage of this shader attachment.</param>
    /// <param name="source">The source code of this shader attachment.</param>
    public ShaderAttachment(ShaderStage stage, string source)
    {
        Stage = stage;
        Source = source;
    }
}