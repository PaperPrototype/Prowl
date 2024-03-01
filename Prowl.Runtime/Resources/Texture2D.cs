using Prowl.Runtime.GraphicsBackend;
using Silk.NET.OpenGL;
using System;
using System.IO;

namespace Prowl.Runtime
{
    /// <summary>
    /// A <see cref="Texture"/> whose image has two dimensions and support for multisampling.
    /// </summary>
    public sealed class Texture2D : Texture, ISerializable
    {
        protected override TextureUsage Usage => TextureUsage.ShaderResource;

        protected override TextureType Type => TextureType.Texture2D;

        /// <summary>The width of this <see cref="Texture2D"/>.</summary>
        public int Width => Internal.Description.Width;

        /// <summary>The height of this <see cref="Texture2D"/>.</summary>
        public int Height => Internal.Description.Height;

        public Texture2D() : base(1, 1, 0, 0, Format.R8G8B8A8_UInt) { }

        /// <summary>
        /// Creates a <see cref="Texture2D"/> with the desired parameters but no image data.
        /// </summary>
        /// <param name="width">The width of the <see cref="Texture2D"/>.</param>
        /// <param name="height">The height of the <see cref="Texture2D"/>.</param>
        /// <param name="generateMipmaps">Whether to generate mipmaps for this <see cref="Texture2D"/>.</param>
        /// <param name="imageFormat">The image format for this <see cref="Texture2D"/>.</param>
        public Texture2D(int width, int height, bool generateMipmaps = false, Format imageFormat = Format.R8G8B8A8_UInt)
            : base(width, height, 0, 0, imageFormat)
        {
            if (generateMipmaps)
                GenerateMipmaps();
        }

        /// <summary>
        /// Sets the data of an area of the <see cref="Texture2D"/>.
        /// </summary>
        /// <param name="ptr">The pointer from which the pixel data will be read.</param>
        /// <param name="rectX">The X coordinate of the first pixel to write.</param>
        /// <param name="rectY">The Y coordinate of the first pixel to write.</param>
        /// <param name="rectWidth">The width of the rectangle of pixels to write.</param>
        /// <param name="rectHeight">The height of the rectangle of pixels to write.</param>
        public unsafe void SetDataPtr(void* ptr, int rectX, int rectY, int rectWidth, int rectHeight)
        {
            ValidateRectOperation(rectX, rectY, rectWidth, rectHeight);
            Graphics.Device.UpdateTexture(Internal, 0, 0, rectX, rectY, 0, rectWidth, rectHeight, 0, ptr);
        }

        /// <summary>
        /// Sets the data of an area of the <see cref="Texture2D"/>.
        /// </summary>
        /// <typeparam name="T">A struct with the same format as this <see cref="Texture2D"/>'s pixels.</typeparam>
        /// <param name="data">A <see cref="Memory{T}"/> containing the new pixel data.</param>
        /// <param name="rectX">The X coordinate of the first pixel to write.</param>
        /// <param name="rectY">The Y coordinate of the first pixel to write.</param>
        /// <param name="rectWidth">The width of the rectangle of pixels to write.</param>
        /// <param name="rectHeight">The height of the rectangle of pixels to write.</param>
        public unsafe void SetData<T>(Memory<T> data, int rectX, int rectY, int rectWidth, int rectHeight) where T : unmanaged
        {
            ValidateRectOperation(rectX, rectY, rectWidth, rectHeight);
            if (data.Length < rectWidth * rectHeight)
                throw new ArgumentException("Not enough pixel data", nameof(data));

            fixed (void* ptr = data.Span)
                Graphics.Device.UpdateTexture(Internal, 0, 0, rectX, rectY, 0, rectWidth, rectHeight, 0, ptr);
        }

        /// <summary>
        /// Sets the data of the entire <see cref="Texture2D"/>.
        /// </summary>
        /// <typeparam name="T">A struct with the same format as this <see cref="Texture2D"/>'s pixels.</typeparam>
        /// <param name="data">A <see cref="ReadOnlySpan{T}"/> containing the new pixel data.</param>
        public void SetData<T>(Memory<T> data) where T : unmanaged
        {
            SetData(data, 0, 0, (int)Width, (int)Height);
        }

        // TODO: GetData

        public int GetSize()
        {
            int size = (int)Width * (int)Height;
            GraphicUtils.BitsPerPixel(Internal.Description.Format);
            return size * (GraphicUtils.BitsPerPixel(Internal.Description.Format) / 8);
        }
        private void ValidateRectOperation(int rectX, int rectY, int rectWidth, int rectHeight)
        {
            if (rectX < 0 || rectY >= Height)
                throw new ArgumentOutOfRangeException(nameof(rectX), rectX, nameof(rectX) + " must be in the range [0, " + nameof(Width) + ")");

            if (rectY < 0 || rectY >= Height)
                throw new ArgumentOutOfRangeException(nameof(rectY), rectY, nameof(rectY) + " must be in the range [0, " + nameof(Height) + ")");

            if (rectWidth <= 0)
                throw new ArgumentOutOfRangeException(nameof(rectWidth), rectWidth, nameof(rectWidth) + " must be greater than 0");

            if (rectHeight <= 0)
                throw new ArgumentOutOfRangeException(nameof(rectHeight), rectHeight, nameof(rectHeight) + "must be greater than 0");

            if (rectWidth > Width - rectX || rectHeight > Height - rectY)
                throw new ArgumentOutOfRangeException("Specified area is outside of the texture's storage");
        }

        public CompoundTag Serialize(TagSerializer.SerializationContext ctx)
        {
            CompoundTag compoundTag = new CompoundTag();

            // Description
            var descTag = TagSerializer.Serialize(Internal.Description, ctx);
            compoundTag.Add("Descriptor", descTag);
            compoundTag.Add("Filter", new IntTag((int)Filter));
            compoundTag.Add("Wrap", new IntTag((int)WrapMode));
            compoundTag.Add("MipMapped", new BoolTag(IsMipmapped));
            Memory<byte> memory = new byte[GetSize()];
            GetData(memory);
            compoundTag.Add("Data", new ByteArrayTag(memory.ToArray()));

            return compoundTag;
        }

        public void Deserialize(CompoundTag value, TagSerializer.SerializationContext ctx)
        {
            var descTag = value["Descriptor"] as CompoundTag;
            var descriptor = TagSerializer.Deserialize<TextureDescription>(descTag, ctx);
            Filter = (TextureFilter)value["Filter"].IntValue;
            WrapMode = (TextureAddress)value["Wrap"].IntValue;
            bool isMipMapped = value["MipMapped"].BoolValue;

            Internal = Graphics.Device.CreateTexture(descriptor);
            Sampler = Graphics.Device.CreateSamplerState(new SamplerStateDescription(Filter, WrapMode, WrapMode, WrapMode, 0, Color.black, 0, float.MaxValue));

            Memory<byte> memory = value["Data"].ByteArrayValue;
            SetData(memory);

            if (isMipMapped)
                GenerateMipmaps();
        }
    }
}
