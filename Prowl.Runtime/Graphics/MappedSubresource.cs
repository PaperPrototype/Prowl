using System;

namespace Prowl.Runtime.GraphicsBackend;

public struct MappedSubresource
{
    public IntPtr DataPtr;

    public MappedSubresource(IntPtr dataPtr)
    {
        DataPtr = dataPtr;
    }
}