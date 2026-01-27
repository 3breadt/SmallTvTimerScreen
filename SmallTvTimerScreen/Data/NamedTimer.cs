// <copyright file="NamedTimer.cs" company="Daniel Dreibrodt">
// Copyright (c) Daniel Dreibrodt. All rights reserved.
// </copyright>

namespace SmallTvTimerScreen.Data;

/// <summary>
/// A timer with a name and end time.
/// </summary>
public class NamedTimer
{
    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the time when the timer ends.
    /// </summary>
    public DateTimeOffset End { get; set; }

    /// <summary>
    /// Gets the remaining time.
    /// </summary>
    public TimeSpan RemainingTime
    {
        get
        {
            var now = DateTimeOffset.Now;
            return now < this.End ? this.End - now : TimeSpan.Zero;
        }
    }
}
