using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Prowl.Runtime.GraphicsBackend.Null;

internal sealed unsafe class NullTexture : InternalTexture
{
    public override bool IsDisposed { get; protected set; }
    public override TextureDescription Description { get; set; }

    public void* Data;

    public NullTexture(TextureDescription description, void* data)
    {
        Validity validity = description.Validity;
        if (!validity.IsValid)
            throw new GraphicsException(validity.Message);
        
        uint sizeInBytes = (uint) (description.Width * description.Height * description.Format.BitsPerPixel() / 8);
        Data = NativeMemory.Alloc(sizeInBytes);
        Unsafe.CopyBlock(Data, data, sizeInBytes);
        
        Description = description;
    }

    public override void Dispose()
    {
        if (IsDisposed)
            return;
        IsDisposed = true;
        
        NativeMemory.Free(Data);
    }
}
