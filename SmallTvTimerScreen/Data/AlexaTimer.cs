// <copyright file="AlexaTimer.cs" company="Daniel Dreibrodt">
// Copyright (c) Daniel Dreibrodt. All rights reserved.
// </copyright>

namespace SmallTvTimerScreen.Data;

using System.Text.Json.Serialization;

/// <summary>
/// Represents an individual Alexa timer entry.
/// </summary>
public class AlexaTimer
{
    /// <summary>
    /// Gets or sets the unique identifier of the timer.
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// Gets or sets the timer label.
    /// </summary>
    public string? Label { get; set; }

    /// <summary>
    /// Gets or sets the status of the timer (ON/OFF).
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public AlexaTimerStatus? Status { get; set; }

    /// <summary>
    /// Gets or sets the type of the timer (e.g., "Timer").
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public AlexaTimerType? Type { get; set; }

    /// <summary>
    /// Gets or sets the remaining time in milliseconds.
    /// </summary>
    public double RemainingTime { get; set; }

    /// <summary>
    /// Gets or sets the last updated date as a Unix timestamp in milliseconds.
    /// </summary>
    public long LastUpdatedDate { get; set; }
}
