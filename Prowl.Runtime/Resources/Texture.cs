using Prowl.Runtime.GraphicsBackend;
using System;

namespace Prowl.Runtime
{
    /// <summary>
    /// Encapsulates an OpenGL texture object. This is the base class for all
    /// texture types and manages some of their internal workings.
    /// </summary>
    public abstract class Texture : EngineObject
    {
        protected abstract TextureUsage Usage { get; }
        protected abstract TextureType Type { get; }

        /// <summary>The internal GL Texture Object.</summary>
        public InternalTexture Internal { get; protected set; }

        public TextureFilter Filter { get; protected set; } = TextureFilter.MinMagMipLinear;
        public TextureAddress WrapMode { get; protected set; } = TextureAddress.Repeat;
        public SamplerState Sampler { get; protected set; }


        /// <summary>Gets whether this <see cref="Texture"/> is mipmapped.</summary>
        public bool IsMipmapped { get; protected set; }

        /// <summary>False if this <see cref="Texture"/> can be mipmapped (depends on texture type).</summary>
        protected bool isNotMipmappable { get; set; }

        /// <summary>Gets whether this <see cref="Texture"/> can be mipmapped (depends on texture type).</summary>
        public bool IsMipmappable => !isNotMipmappable;

        /// <summary>
        /// Creates a <see cref="Texture"/> with specified <see cref="TextureType"/>.
        /// </summary>
        /// <param name="type">The type of texture (or texture target) the texture will be.</param>
        /// <param name="imageFormat">The type of image format this texture will store.</param>
        internal Texture(int Width, int Height, int Depth, int ArraySize, Format imageFormat) : base("New Texture")
        {
            IsMipmapped = false;
            isNotMipmappable = !IsTextureTypeMipmappable(Type);


            TextureDescription textureDescription = new TextureDescription {
                Width = Width,
                Height = Height,
                Depth = Depth,
                Format = imageFormat,
                Usage = Usage,
                TextureType = Type,
                ArraySize = ArraySize,
                
            };

            Internal = Graphics.Device.CreateTexture(textureDescription);
            Sampler = Graphics.Device.CreateSamplerState(new SamplerStateDescription(Filter, WrapMode, WrapMode, WrapMode, 0, Color.black, 0, float.MaxValue));
        }

        /// <summary>
        /// Sets this <see cref="Texture"/>'s filter.
        /// </summary>
        /// <param name="filter">The desired filter for the <see cref="Texture"/>.</param>
        public void SetTextureFilters(TextureFilter filter)
        {
            Sampler = Graphics.Device.CreateSamplerState(new SamplerStateDescription(filter, WrapMode, WrapMode, WrapMode, 0, Color.black, 0, float.MaxValue));
            Filter = filter;
        }

        /// <summary>
        /// Sets the texture coordinate wrapping modes for when a texture is sampled outside the [0, 1] range.
        /// </summary>
        public void SetWrapModes(TextureAddress wrap)
        {
            Sampler = Graphics.Device.CreateSamplerState(new SamplerStateDescription(Filter, wrap, wrap, WrapMode, 0, Color.black, 0, float.MaxValue));
            WrapMode = wrap;
        }

        /// <summary>
        /// Generates mipmaps for this <see cref="Texture"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException"/>
        public void GenerateMipmaps()
        {
            if (isNotMipmappable)
                throw new InvalidOperationException(string.Concat("This texture type is not mipmappable! Type: ", Type.ToString()));

            Graphics.Device.GenerateMipmaps(Internal);
            IsMipmapped = true;
        }

        public void Dispose()
        {
            Internal.Dispose();
            Sampler.Dispose();
        }

        /// Gets whether the specified <see cref="TextureType"/> type is mipmappable.
        /// </summary>
        public static bool IsTextureTypeMipmappable(TextureType textureType)
        {
            return textureType == TextureType.Texture1D || textureType == TextureType.Texture2D || textureType == TextureType.Texture3D
                || textureType == TextureType.Cubemap;
        }
    }
}
