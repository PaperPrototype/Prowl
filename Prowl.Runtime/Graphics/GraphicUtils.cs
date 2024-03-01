﻿using System;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Prowl.Runtime.GraphicsBackend;

/// <summary>
/// Various handy utilities you may use when developing with Prowl.
/// </summary>
public static class GraphicUtils
{
    #region Public API
    
    /// <summary>
    /// Combine multiple textures into one array. Prowl texture functions only support a single byte array, however certain
    /// texture types (array textures, cubemaps) require you to have <i>multiple</i> of these byte arrays. This function
    /// performs a fast block copy of the arrays into a single array.
    /// </summary>
    /// <param name="data">The data arrays to combine.</param>
    /// <typeparam name="T">The data type.</typeparam>
    /// <returns>The combined data.</returns>
    public static unsafe T[] Combine<T>(params T[][] data) where T : unmanaged
    {
        int totalSize = 0;
        for (int i = 0; i < data.Length; i++)
            totalSize += data[i].Length;
        T[] result = new T[totalSize];

        totalSize = 0;
        fixed (void* ptr = result)
        {
            for (int i = 0; i < data.Length; i++)
            {
                fixed (void* dataPtr = data[i])
                    Unsafe.CopyBlock((byte*) ptr + totalSize, dataPtr, (uint) (data[i].Length * sizeof(T)));

                totalSize += data[i].Length;
            }
        }

        return result;
    }

    /// <summary>
    /// Calculate the total number of mipmap levels (including the most detailed mip) from the given width, height, and depth.
    /// </summary>
    /// <param name="width">The width of the texture.</param>
    /// <param name="height">The height of the texture.</param>
    /// <param name="depth">The depth of the texture.</param>
    /// <returns>The number of mip levels.</returns>
    /// <remarks>For the result to be accurate, you should use the width and height of the most detailed mip level.</remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CalculateMipLevels(int width, int height, int depth)
    {
        return (int) Math.Floor(Math.Log2(Math.Max(width, Math.Max(height, depth)))) + 1;
    }

    /// <summary>
    /// Gets the bits per pixel of the given format. (For example, R8G8B8A8_UNorm would be 32-bits).
    /// </summary>
    /// <param name="format"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static int BitsPerPixel(this Format format)
    {
        int bitsPerPixel = 0;

        switch (format)
        {
            case Format.R8_UNorm:
            case Format.R8_SNorm:
            case Format.R8_SInt:
            case Format.R8_UInt:
                bitsPerPixel = 8;
                break;

            case Format.R8G8_UNorm:
            case Format.R8G8_SNorm:
            case Format.R8G8_SInt:
            case Format.R8G8_UInt:
            case Format.R16_UNorm:
            case Format.R16_SNorm:
            case Format.R16_SInt:
            case Format.R16_UInt:
            case Format.R16_Float:
            case Format.D16_UNorm:
                bitsPerPixel = 16;
                break;

            case Format.R8G8B8A8_UNorm:
            case Format.R8G8B8A8_UNorm_SRgb:
            case Format.R8G8B8A8_SNorm:
            case Format.R8G8B8A8_SInt:
            case Format.R8G8B8A8_UInt:
            case Format.B8G8R8A8_UNorm:
            case Format.B8G8R8A8_UNorm_SRgb:
            case Format.R16G16_UNorm:
            case Format.R16G16_SNorm:
            case Format.R16G16_SInt:
            case Format.R16G16_UInt:
            case Format.R16G16_Float:
            case Format.R32_SInt:
            case Format.R32_UInt:
            case Format.R32_Float:
            case Format.D24_UNorm_S8_UInt:
            case Format.D32_Float:
                bitsPerPixel = 32;
                break;

            case Format.R16G16B16A16_UNorm:
            case Format.R16G16B16A16_SNorm:
            case Format.R16G16B16A16_SInt:
            case Format.R16G16B16A16_UInt:
            case Format.R16G16B16A16_Float:
            case Format.R32G32_SInt:
            case Format.R32G32_UInt:
            case Format.R32G32_Float:
                bitsPerPixel = 64;
                break;

            case Format.R32G32B32_SInt:
            case Format.R32G32B32_UInt:
            case Format.R32G32B32_Float:
                bitsPerPixel = 96;
                break;

            case Format.R32G32B32A32_SInt:
            case Format.R32G32B32A32_UInt:
            case Format.R32G32B32A32_Float:
                bitsPerPixel = 128;
                break;

            case Format.BC1_UNorm:
            case Format.BC1_UNorm_SRgb:
            case Format.BC4_UNorm:
            case Format.BC4_SNorm:
                bitsPerPixel = 4;
                break;

            case Format.BC2_UNorm:
            case Format.BC2_UNorm_SRgb:
            case Format.BC3_UNorm:
            case Format.BC3_UNorm_SRgb:
            case Format.BC5_UNorm:
            case Format.BC5_SNorm:
            case Format.BC6H_UF16:
            case Format.BC6H_SF16:
            case Format.BC7_UNorm:
            case Format.BC7_UNorm_SRgb:
                bitsPerPixel = 8;
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(format), format, null);
        }

        return bitsPerPixel;
    }

    #endregion
    
    #region Internal API

    internal static int CalculatePitch(Format format, int width, out int bitsPerPixel)
    {
        int pitch;
        bitsPerPixel = BitsPerPixel(format);

        if (format is >= Format.BC1_UNorm and <= Format.BC7_UNorm_SRgb)
        {
            switch (format)
            {
                case Format.BC1_UNorm:
                case Format.BC1_UNorm_SRgb:
                case Format.BC4_UNorm:
                case Format.BC4_SNorm:
                    pitch = PitchBlock(width, 8);
                    break;
                case Format.BC2_UNorm:
                case Format.BC2_UNorm_SRgb:
                case Format.BC3_UNorm:
                case Format.BC3_UNorm_SRgb:
                case Format.BC5_UNorm:
                case Format.BC5_SNorm:
                case Format.BC6H_UF16:
                case Format.BC6H_SF16:
                case Format.BC7_UNorm:
                case Format.BC7_UNorm_SRgb:
                    pitch = PitchBlock(width, 16);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(format), format, null);
            }
        }
        else
            pitch = Pitch(width, bitsPerPixel);

        return pitch;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int PitchBlock(int width, int blockSize) => Max(1, (width + 3) >> 2) * blockSize;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int Pitch(int width, int bitsPerPixel) => (width * bitsPerPixel + 7) >> 3;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int Max(int l, int r) => l < r ? r : l;

    #endregion
}