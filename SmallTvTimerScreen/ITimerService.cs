// <copyright file="ITimerService.cs" company="Daniel Dreibrodt">
// Copyright (c) Daniel Dreibrodt. All rights reserved.
// </copyright>

namespace SmallTvTimerScreen;

using SmallTvTimerScreen.Data;

/// <summary>
/// Interface for managing active timers.
/// </summary>
public interface ITimerService
{
    /// <summary>
    /// Gets a value indicating whether a timer is currently active.
    /// </summary>
    bool IsTimerActive { get; }

    /// <summary>
    /// Clears the active timers.
    /// </summary>
    void ClearTimers();

    /// <summary>
    /// Sets the active timers.
    /// </summary>
    /// <param name="timers">The timers.</param>
    void SetTimers(IList<NamedTimer> timers);
}
