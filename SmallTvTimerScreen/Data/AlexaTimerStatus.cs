// <copyright file="AlexaTimerStatus.cs" company="Daniel Dreibrodt">
// Copyright (c) Daniel Dreibrodt. All rights reserved.
// </copyright>

namespace SmallTvTimerScreen.Data;

/// <summary>
/// Status of an Alexa timer.
/// </summary>
public enum AlexaTimerStatus
{
    /// <summary>
    /// An unknown status.
    /// </summary>
    Unknown,

    /// <summary>
    /// Timer is off.
    /// </summary>
    Off,

    /// <summary>
    /// Timer is on.
    /// </summary>
    On,
}
