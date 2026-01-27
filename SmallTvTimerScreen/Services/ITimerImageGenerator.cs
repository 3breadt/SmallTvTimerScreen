// <copyright file="ITimerImageGenerator.cs" company="Daniel Dreibrodt">
// Copyright (c) Daniel Dreibrodt. All rights reserved.
// </copyright>

namespace SmallTvTimerScreen.Services;

using SmallTvTimerScreen.Data;

/// <summary>
/// Interface for generating timer images.
/// </summary>
public interface ITimerImageGenerator
{
    /// <summary>
    /// Creates a timer image as a GIF.
    /// </summary>
    /// <param name="timers">The timers to display.</param>
    /// <returns>The GIF image data.</returns>
    byte[] CreateTimerImageAsGif(IReadOnlyList<NamedTimer> timers);

    /// <summary>
    /// Creates a timer image as a JPEG.
    /// </summary>
    /// <param name="timers">The timers to display.</param>
    /// <returns>The JPEG image data.</returns>
    byte[] CreateTimerImageAsJpeg(IReadOnlyList<NamedTimer> timers);
}
