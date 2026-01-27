// <copyright file="AlarmsBrief.cs" company="Daniel Dreibrodt">
// Copyright (c) Daniel Dreibrodt. All rights reserved.
// </copyright>

namespace SmallTvTimerScreen.Data;

using System.Text.Json.Serialization;

/// <summary>
/// Contains the active and all timer lists.
/// </summary>
public class AlarmsBrief
{
    /// <summary>
    /// Gets or sets the list of active timers.
    /// </summary>
    public List<AlexaTimer>? Active { get; set; }

    /// <summary>
    /// Gets or sets the list of all timers.
    /// </summary>
    public List<AlexaTimer>? All { get; set; }
}
