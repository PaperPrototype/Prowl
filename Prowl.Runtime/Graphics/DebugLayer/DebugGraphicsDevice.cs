using Prowl.Runtime.GraphicsBackend.ShaderCompiler;
using System;
using System.Drawing;
using System.Runtime.CompilerServices;

namespace Prowl.Runtime.GraphicsBackend.DebugLayer;

internal sealed unsafe class DebugGraphicsDevice : GraphicsDevice
{ 
    public static GraphicsDevice Device;

    private bool _vertexBufferSet;
    private bool _indexBufferSet;
    private bool _shaderSet;

    public DebugGraphicsDevice(GraphicsDevice device)
    {
        Device = device;
        
        Debug.Log("Debug graphics device initialized.");
        Debug.Log($"Adapter: {Adapter.Name}");
        Debug.Log($"Backend: {Api}");
    }

    public override GraphicsApi Api => Device.Api;

    public override Swapchain Swapchain => Device.Swapchain;

    public override GraphicsAdapter Adapter => Device.Adapter;

    public override Rectangle Viewport
    {
        get => Device.Viewport;
        set => Device.Viewport = value;
    }

    public override Rectangle Scissor
    {
        get => Device.Scissor;
        set => Device.Scissor = value;
    }
    
    public override void ClearColorBuffer(Color color)
    {
        Device.ClearColorBuffer(color);
    }

    public override void ClearColorBuffer(Vector4 color)
    {
       Device.ClearColorBuffer(color);
    }

    public override void ClearColorBuffer(float r, float g, float b, float a)
    {
        Device.ClearColorBuffer(r, g, b, a);
    }

    public override void ClearDepthStencilBuffer(ClearFlags flags, float depth, byte stencil)
    {
        Device.ClearDepthStencilBuffer(flags, depth, stencil);
    }

    public override GraphicsBuffer CreateBuffer<T>(BufferType bufferType, T[] data, bool dynamic = false)
    {
        fixed (void* ptr = data)
            return new DebugGraphicsBuffer(bufferType, (uint) (data.Length * sizeof(T)), ptr, dynamic);
    }

    public override GraphicsBuffer CreateBuffer<T>(BufferType bufferType, T data, bool dynamic = false)
    {
        return new DebugGraphicsBuffer(bufferType, (uint) sizeof(T), Unsafe.AsPointer(ref data), dynamic);
    }

    public override GraphicsBuffer CreateBuffer(BufferType bufferType, uint sizeInBytes, bool dynamic = false)
    {
        return new DebugGraphicsBuffer(bufferType, sizeInBytes, null, dynamic);
    }

    public override GraphicsBuffer CreateBuffer(BufferType bufferType, uint sizeInBytes, IntPtr data, bool dynamic = false)
    {
        return new DebugGraphicsBuffer(bufferType, sizeInBytes, (void*) data, dynamic);
    }

    public override unsafe GraphicsBuffer CreateBuffer(BufferType bufferType, uint sizeInBytes, void* data, bool dynamic = false)
    {
        return new DebugGraphicsBuffer(bufferType, sizeInBytes, data, dynamic);
    }

    public override InternalTexture CreateTexture(TextureDescription description)
    {
        return new DebugTexture(description, null);
    }

    public override InternalTexture CreateTexture<T>(TextureDescription description, T[] data)
    {
        DebugUtils.CheckIfValid(description, data);
        
        fixed (void* ptr = data)
            return new DebugTexture(description, ptr);
    }

    public override InternalTexture CreateTexture<T>(TextureDescription description, T[][] data)
    {
        T[] combined = GraphicUtils.Combine(data);
        
        DebugUtils.CheckIfValid(description, combined);
        
        fixed (void* ptr = combined)
            return new DebugTexture(description, ptr);
    }

    public override InternalTexture CreateTexture(TextureDescription description, IntPtr data)
    {
        return new DebugTexture(description, (void*) data);
    }

    public override unsafe InternalTexture CreateTexture(TextureDescription description, void* data)
    {
        return new DebugTexture(description, data);
    }

    public override InternalShader CreateShader(ShaderAttachment[] attachments, SpecializationConstant[] constants = null)
    {
        return new DebugShader(attachments, constants);
    }

    public override InputLayout CreateInputLayout(params InputLayoutDescription[] inputLayoutDescriptions)
    {
        return new DebugInputLayout(inputLayoutDescriptions);
    }

    public override RasterizerState CreateRasterizerState(RasterizerStateDescription description)
    {
        return new DebugRasterizerState(description);
    }

    public override BlendState CreateBlendState(BlendStateDescription description)
    {
        return new DebugBlendState(description);
    }

    public override DepthStencilState CreateDepthStencilState(DepthStencilStateDescription description)
    {
        return new DebugDepthStencilState(description);
    }

    public override SamplerState CreateSamplerState(SamplerStateDescription description)
    {
        return new DebugSamplerState(description);
    }

    public override Framebuffer CreateFramebuffer(params FramebufferAttachment[] attachments)
    {
        return new DebugFramebuffer(attachments);
    }

    public override void UpdateBuffer<T>(GraphicsBuffer buffer, uint offsetInBytes, T[] data)
    {
        fixed (void* ptr = data)
            ((DebugGraphicsBuffer) buffer).Update(offsetInBytes, (uint) (data.Length * sizeof(T)), ptr);
    }

    public override void UpdateBuffer<T>(GraphicsBuffer buffer, uint offsetInBytes, T data)
    {
        ((DebugGraphicsBuffer) buffer).Update(offsetInBytes, (uint) sizeof(T), Unsafe.AsPointer(ref data));
    }

    public override void UpdateBuffer(GraphicsBuffer buffer, uint offsetInBytes, uint sizeInBytes, IntPtr data)
    {
        ((DebugGraphicsBuffer) buffer).Update(offsetInBytes, sizeInBytes, (void*) data);
    }

    public override unsafe void UpdateBuffer(GraphicsBuffer buffer, uint offsetInBytes, uint sizeInBytes, void* data)
    {
        ((DebugGraphicsBuffer) buffer).Update(offsetInBytes, sizeInBytes, data);
    }

    public override void UpdateTexture<T>(InternalTexture texture, int mipLevel, int arrayIndex, int x, int y, int z, int width, int height,
        int depth, T[] data)
    {
        fixed (void* ptr = data)
            ((DebugTexture) texture).Update(mipLevel, arrayIndex, x, y, z, width, height, depth, ptr);
    }

    public override void UpdateTexture(InternalTexture texture, int mipLevel, int arrayIndex, int x, int y, int z, int width, int height, int depth,
        IntPtr data)
    {
        ((DebugTexture) texture).Update(mipLevel, arrayIndex, x, y, z, width, height, depth, (void*) data);
    }

    public override unsafe void UpdateTexture(InternalTexture texture, int mipLevel, int arrayIndex, int x, int y, int z, int width, int height,
        int depth, void* data)
    {
        ((DebugTexture) texture).Update(mipLevel, arrayIndex, x, y, z, width, height, depth, data);
    }

    public override MappedSubresource MapResource(MappableResource resource, MapMode mode)
    {
        return resource.Map(mode);
    }

    public override void UnmapResource(MappableResource resource)
    {
        resource.Unmap();
    }

    public override void SetShader(InternalShader shader)
    {
        if (shader.IsDisposed)
            Debug.LogError("Attempted to set a disposed shader!");

        _shaderSet = true;
        Device.SetShader(((DebugShader) shader).Shader);
    }

    public override void SetTexture(uint bindingSlot, InternalTexture texture, SamplerState samplerState)
    {
        if (texture.IsDisposed)
            Debug.LogError("Attempted to set a disposed texture!");

        Device.SetTexture(bindingSlot, ((DebugTexture) texture).Texture, ((DebugSamplerState) samplerState).SamplerState);
    }

    public override void SetRasterizerState(RasterizerState state)
    {
        if (state.IsDisposed)
            Debug.LogError("Attempted to set a disposed rasterizer state!");

        Device.SetRasterizerState(((DebugRasterizerState) state).RasterizerState);
    }

    public override void SetBlendState(BlendState state)
    {
        if (state.IsDisposed)
            Debug.LogError("Attempted to set a disposed blend state!");

        Device.SetBlendState(((DebugBlendState) state).BlendState);
    }

    public override void SetDepthStencilState(DepthStencilState state, int stencilRef = 0)
    {
        if (state.IsDisposed)
            Debug.LogError("Attempted to set a disposed depth stencil state!");

        Device.SetDepthStencilState(((DebugDepthStencilState) state).DepthStencilState, stencilRef);
    }

    public override void SetPrimitiveType(PrimitiveType type)
    {
        Device.SetPrimitiveType(type);
    }

    public override void SetInputLayout(InputLayout layout)
    {
        if (layout.IsDisposed)
            Debug.LogError("Attempted to set a disposed input layout!");
        
        Device.SetInputLayout(((DebugInputLayout) layout).InputLayout);
    }

    public override void SetVertexBuffer(uint slot, GraphicsBuffer buffer, uint stride)
    {
        if (buffer.IsDisposed)
            Debug.LogError("Attempted to set a disposed buffer!");

        DebugGraphicsBuffer dBuffer = (DebugGraphicsBuffer) buffer;
        if (dBuffer.BufferType != BufferType.VertexBuffer)
            Debug.LogError($"Expected VertexBuffer, buffer is an {dBuffer.BufferType} instead.");

        _vertexBufferSet = true;
        Device.SetVertexBuffer(slot, dBuffer.Buffer, stride);
    }

    public override void SetIndexBuffer(GraphicsBuffer buffer, IndexType type)
    {
        if (buffer.IsDisposed)
            Debug.LogError("Attempted to set a disposed buffer!");

        DebugGraphicsBuffer dBuffer = (DebugGraphicsBuffer) buffer;
        if (dBuffer.BufferType != BufferType.IndexBuffer)
            Debug.LogError($"Expected IndexBuffer, buffer is an {dBuffer.BufferType} instead.");

        _indexBufferSet = true;
        Device.SetIndexBuffer(dBuffer.Buffer, type);
    }

    public override void SetUniformBuffer(uint bindingSlot, GraphicsBuffer buffer)
    {
        if (buffer.IsDisposed)
            Debug.LogError("Attempted to set a disposed buffer!");

        DebugGraphicsBuffer dBuffer = (DebugGraphicsBuffer) buffer;
        if (dBuffer.BufferType != BufferType.UniformBuffer)
            Debug.LogError($"Expected UniformBuffer, buffer is an {dBuffer.BufferType} instead.");
        
        Device.SetUniformBuffer(bindingSlot, dBuffer.Buffer);
    }

    public override void SetFramebuffer(Framebuffer framebuffer)
    {
        if (framebuffer == null)
        {
            Device.SetFramebuffer(null);
            return;
        }

        DebugFramebuffer dFb = (DebugFramebuffer) framebuffer;

        if (dFb.IsDisposed)
            Debug.LogError("Attempted to set a disposed framebuffer!");
        
        foreach (FramebufferAttachment attachment in dFb.Attachments)
        {
            if (attachment.Texture.IsDisposed)
                Debug.LogError("Attached framebuffer texture has been disposed since the framebuffer was created.");
        }
        
        Device.SetFramebuffer(dFb.Framebuffer);
    }

    public override void Draw(uint vertexCount)
    {
        if (!_shaderSet)
            Debug.LogError("Attempted to draw, however no shader has been set.");
        
        //if (!_vertexBufferSet)
        //    Debug.LogError("Attempted to draw, however no vertex buffer has been set.");
        
        Device.Draw(vertexCount);
    }

    public override void Draw(uint vertexCount, int startVertex)
    {
        if (!_shaderSet)
            Debug.LogError("Attempted to draw, however no shader has been set.");
        
        //if (!_vertexBufferSet)
        //    Debug.LogError("Attempted to draw, however no vertex buffer has been set.");
        
        //if (startVertex >= vertexCount)
        //    Debug.LogError($"The vertex count was {vertexCount}, but the start vertex was {startVertex}.");
        
        Device.Draw(vertexCount, startVertex);
    }

    public override void DrawIndexed(uint indexCount)
    {
        if (!_shaderSet)
            Debug.LogError("Attempted to draw indexed, however no shader has been set.");
        
        if (!_vertexBufferSet)
            Debug.LogError("Attempted to draw indexed, however no vertex buffer has been set.");
        
        if (!_indexBufferSet)
            Debug.LogError("Attempted to draw indexed, however no index buffer has been set.");
        
        Device.DrawIndexed(indexCount);
    }

    public override void DrawIndexed(uint indexCount, int startIndex)
    {
        if (!_shaderSet)
            Debug.LogError("Attempted to draw indexed, however no shader has been set.");
        
        if (!_vertexBufferSet)
            Debug.LogError("Attempted to draw indexed, however no vertex buffer has been set.");
        
        if (!_indexBufferSet)
            Debug.LogError("Attempted to draw indexed, however no index buffer has been set.");
        
        //if (startIndex >= indexCount)
        //    Debug.LogError($"The index count was {indexCount}, but the start index was {startIndex}.");
        
        Device.DrawIndexed(indexCount, startIndex);
    }

    public override void DrawIndexed(uint indexCount, int startIndex, int baseVertex)
    {
        if (!_shaderSet)
            Debug.LogError("Attempted to draw indexed, however no shader has been set.");
        
        if (!_vertexBufferSet)
            Debug.LogError("Attempted to draw indexed, however no vertex buffer has been set.");
        
        if (!_indexBufferSet)
            Debug.LogError("Attempted to draw indexed, however no index buffer has been set.");
        
        //if (startIndex >= indexCount)
        //    Debug.LogError($"The index count was {indexCount}, but the start index was {startIndex}.");
        
        Device.DrawIndexed(indexCount, startIndex, baseVertex);
    }

    public override void DrawIndexedInstanced(uint indexCount, uint instanceCount)
    {
        if (!_shaderSet)
            Debug.LogError("Attempted to draw indexed instanced, however no shader has been set.");
        
        if (!_vertexBufferSet)
            Debug.LogError("Attempted to draw indexed instanced, however no vertex buffer has been set.");
        
        if (!_indexBufferSet)
            Debug.LogError("Attempted to draw indexed instanced, however no index buffer has been set.");
        
        Device.DrawIndexedInstanced(indexCount, instanceCount);
    }

    public override void Present(int swapInterval)
    {
        if (swapInterval > 4)
            Debug.LogError($"Swap interval should be a maximum of 4, however an interval of {swapInterval} was provided.");

        Device.Present(swapInterval);
    }

    public override void ResizeSwapchain(Size newSize)
    {
        Device.ResizeSwapchain(newSize);
    }

    public override void GenerateMipmaps(InternalTexture texture)
    {
        DebugTexture dTexture = (DebugTexture) texture;
        
        if (dTexture.Description.Format is >= Format.BC1_UNorm and <= Format.BC7_UNorm_SRgb)
            Debug.LogError("Cannot generate mipmaps for block compressed textures.");
        
        if (dTexture.Description.MipLevels == 1)
            Debug.LogWarning("Attempting to generate mipmaps for a texture that does not support mipmaps.");

        Device.GenerateMipmaps(dTexture.Texture);
    }

    public override void Dispatch(uint groupCountX, uint groupCountY, uint groupCountZ)
    {
        Device.Dispatch(groupCountX, groupCountY, groupCountZ);
    }

    public override void Flush()
    {
        Device.Flush();
    }

    public override void Dispose()
    {
        Device.Dispose();
        Debug.Log(DebugMetrics.GetString());
    }
}