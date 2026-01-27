// <copyright file="ImageSettings.cs" company="Daniel Dreibrodt">
// Copyright (c) Daniel Dreibrodt. All rights reserved.
// </copyright>

namespace SmallTvTimerScreen;

/// <summary>
/// Settings for generating the timer image.
/// </summary>
public class ImageSettings
{
    /// <summary>
    /// The name of the application settings section containing the image settings.
    /// </summary>
    public const string ConfigurationSection = "Image";

    /// <summary>
    /// Gets or sets the path to the font for the time display.
    /// </summary>
    /// <remarks>
    /// Either <see cref="TimeFontPath"/> or <see cref="TimeFontFamily"/> should be set. If both are set, <see cref="TimeFontPath"/> takes precedence.
    /// </remarks>
    public string? TimeFontPath { get; set; }

    /// <summary>
    /// Gets or sets the font family for the time display.
    /// </summary>
    /// <remarks>
    /// Either <see cref="TimeFontPath"/> or <see cref="TimeFontFamily"/> should be set. If both are set, <see cref="TimeFontPath"/> takes precedence.
    /// </remarks>
    public string? TimeFontFamily { get; set; }

    /// <summary>
    /// Gets or sets the font size for the time display in points. Defaults to 48 pt.
    /// </summary>
    public float? TimeFontSize { get; set; }

    /// <summary>
    /// Gets or sets the color for the time display in hexadecimal notation.
    /// </summary>
    public string? TimeFontColor { get; set; }

    /// <summary>
    /// Gets or sets the path to the font for the timer name display.
    /// </summary>
    /// <remarks>
    /// Either <see cref="NameFontPath"/> or <see cref="NameFontFamily"/> should be set. If both are set, <see cref="NameFontPath"/> takes precedence.
    /// </remarks>
    public string? NameFontPath { get; set; }

    /// <summary>
    /// Gets or sets the font family for the timer name display.
    /// </summary>
    /// <remarks>
    /// Either <see cref="NameFontPath"/> or <see cref="NameFontFamily"/> should be set. If both are set, <see cref="NameFontPath"/> takes precedence.
    /// </remarks>
    public string? NameFontFamily { get; set; }

    /// <summary>
    /// Gets or sets the font size for the timer name display in points.
    /// </summary>
    public float? NameFontSize { get; set; }

    /// <summary>
    /// Gets or sets the color of the timer name display in hexadecimal notation.
    /// </summary>
    /// <value>
    /// The color of the name font.
    /// </value>
    public string? NameFontColor { get; set; }

    /// <summary>
    /// Gets or sets the path to a background image for the timer display.
    /// </summary>
    public string? BackgroundImagePath { get; set; }

    /// <summary>
    /// Gets or sets the start color of the background gradient for the timer display. The gradient is from top to bottom.
    /// </summary>
    public string? BackgroundGradientStartColor { get; set; }

    /// <summary>
    /// Gets or sets the end color of the background gradient for the timer display. The gradient is from top to bottom.
    /// </summary>
    public string? BackgroundGradientEndColor { get; set; }
}
