// <copyright file="AlexaNextTimerValue.cs" company="Daniel Dreibrodt">
// Copyright (c) Daniel Dreibrodt. All rights reserved.
// </copyright>

namespace SmallTvTimerScreen.Data;

using System.Text.Json.Serialization;

/// <summary>
/// Represents the attributes of an Alexa media player next timer entity in Home Assistant.
/// </summary>
public class NextTimerAttributes
{
    /// <summary>
    /// Gets or sets the timestamp when the data was processed.
    /// </summary>
    [JsonPropertyName("process_timestamp")]
    public DateTimeOffset? ProcessTimestamp { get; set; }

    /// <summary>
    /// Gets or sets the prior value.
    /// </summary>
    [JsonPropertyName("prior_value")]
    public DateTimeOffset? PriorValue { get; set; }

    /// <summary>
    /// Gets or sets the total number of active timers.
    /// </summary>
    [JsonPropertyName("total_active")]
    public int TotalActive { get; set; }

    /// <summary>
    /// Gets or sets the total number of all timers.
    /// </summary>
    [JsonPropertyName("total_all")]
    public int TotalAll { get; set; }

    /// <summary>
    /// Gets or sets the status (ON/OFF).
    /// </summary>
    [JsonPropertyName("status")]
    public string? Status { get; set; }

    /// <summary>
    /// Gets or sets the dismissed timestamp.
    /// </summary>
    [JsonPropertyName("dismissed")]
    public DateTimeOffset? Dismissed { get; set; }

    /// <summary>
    /// Gets or sets the timer name.
    /// </summary>
    [JsonPropertyName("timer")]
    public string? Timer { get; set; }

    /// <summary>
    /// Gets or sets the alarms brief information.
    /// </summary>
    [JsonPropertyName("brief")]
    public AlarmsBrief? AlarmsBrief { get; set; }
}
