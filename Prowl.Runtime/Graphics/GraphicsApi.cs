﻿using System;

namespace Prowl.Runtime.GraphicsBackend;

/// <summary>
/// The backend graphics API used for a <see cref="GraphicsDevice"/>.
/// </summary>
public enum GraphicsApi
{
    /// <summary>
    /// OpenGL 4.3, GLSL version 430.
    /// </summary>
    OpenGL,
    
    /// <summary>
    /// OpenGL ES 3.0, ESSL version 300.
    /// </summary>
    OpenGLES,

    /// <summary>
    /// Null device. Does not do any rendering.
    /// </summary>
    Null
}

public static class GraphicsApiExtensions
{
    /// <summary>
    /// Get the "friendly" name of the API.
    /// </summary>
    /// <param name="api">The API.</param>
    /// <returns>The "friendly" name.</returns>
    /// <exception cref="ArgumentOutOfRangeException">An invalid API was provided.</exception>
    public static string ToFriendlyString(this GraphicsApi api)
    {
        return api switch
        {
            GraphicsApi.OpenGL => "OpenGL",
            GraphicsApi.OpenGLES => "OpenGL ES",
            GraphicsApi.Null => "Null",
            _ => throw new ArgumentOutOfRangeException(nameof(api), api, null)
        };
    }
}