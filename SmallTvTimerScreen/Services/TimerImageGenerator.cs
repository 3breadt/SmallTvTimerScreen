// <copyright file="TimerImageGenerator.cs" company="Daniel Dreibrodt">
// Copyright (c) Daniel Dreibrodt. All rights reserved.
// </copyright>

namespace SmallTvTimerScreen.Services;

using Microsoft.Extensions.Options;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing.Processors.Quantization;

using SkiaSharp;

using SmallTvTimerScreen;
using SmallTvTimerScreen.Data;

/// <summary>
/// Generates timer images for display on the SmallTV device.
/// </summary>
/// <seealso cref="IDisposable" />
public sealed class TimerImageGenerator : ITimerImageGenerator, IDisposable
{
    private const int Height = 240;
    private const int Width = 240;
    private readonly ImageSettings settings;
    private readonly TextSettings timeTextSettings;
    private readonly TextSettings nameTextSettings;
    private readonly SKColor gradientStartColor;
    private readonly SKColor gradientEndColor;

    /// <summary>
    /// Initializes a new instance of the <see cref="TimerImageGenerator"/> class.
    /// </summary>
    /// <param name="imageOptions">The image options.</param>
    public TimerImageGenerator(IOptions<ImageSettings> imageOptions)
    {
        this.settings = imageOptions.Value;
        this.timeTextSettings = new TextSettings(
            this.settings.TimeFontPath,
            this.settings.TimeFontFamily,
            this.settings.TimeFontSize ?? 48,
            this.settings.TimeFontColor ?? "#FFFFFF");
        this.nameTextSettings = new TextSettings(
            this.settings.NameFontPath,
            this.settings.NameFontFamily,
            this.settings.NameFontSize ?? 18,
            this.settings.NameFontColor ?? "#FFFFFF");

        this.gradientStartColor = SKColor.Parse(
            this.settings.BackgroundGradientStartColor ?? "#05A0D1");
        this.gradientEndColor = SKColor.Parse(
            this.settings.BackgroundGradientEndColor ?? "#232F3E");
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        this.timeTextSettings.Dispose();
        this.nameTextSettings.Dispose();
    }

    /// <summary>
    /// Creates the timer image as JPEG.
    /// </summary>
    /// <param name="timers">The timers.</param>
    /// <returns>The JPEG data.</returns>
    public byte[] CreateTimerImageAsJpeg(IReadOnlyList<NamedTimer> timers)
    {
        using var image = this.CreateImage(timers);
        using var data = image.Encode(SKEncodedImageFormat.Jpeg, 95);
        return data.ToArray();
    }

    /// <summary>
    /// Creates the timer image as GIF.
    /// </summary>
    /// <param name="timers">The timers.</param>
    /// <returns>The GIF data.</returns>
    public byte[] CreateTimerImageAsGif(IReadOnlyList<NamedTimer> timers)
    {
        using var image = this.CreateImage(timers);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);

        using var ms = new MemoryStream(data.ToArray());
        using var img = Image.Load<Rgba32>(ms);

        var gifEncoder = new GifEncoder
        {
            ColorTableMode = GifColorTableMode.Global,
            Quantizer = new WuQuantizer(),
        };

        using var gifStream = new MemoryStream();
        img.Save(gifStream, gifEncoder);
        return gifStream.ToArray();
    }

    private static string FormatTimer(TimeSpan timeSpan) => timeSpan switch
    {
        { TotalHours: > 99 } => $"{(int)timeSpan.TotalHours} h",
        { TotalHours: >= 1 } => $"{(int)timeSpan.TotalHours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}",
        _ => $"{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}",
    };

    private SKImage CreateImage(IReadOnlyList<NamedTimer> timers)
    {
        using var surface = SKSurface.Create(new SKImageInfo(Height, Width, SKColorType.Rgba8888));
        var canvas = surface.Canvas;
        this.DrawBackground(canvas);

        const int Margin = 20;
        const int MaxTextWidth = Width - (2 * Margin);
        const float HorizontalCenter = Width / 2;
        const float TimeYRelative = 0.5f;
        const float NameYRelative = 0.8f;

        if (timers.Count == 1)
        {
            const float TimeY = Height * TimeYRelative;
            const float NameY = Height * NameYRelative;
            var name = timers[0].Name;
            var remaining = FormatTimer(timers[0].RemainingTime);
            this.timeTextSettings.DrawCentered(canvas, remaining, HorizontalCenter, TimeY, MaxTextWidth, 1, true);
            if (!string.IsNullOrEmpty(name))
            {
                this.nameTextSettings.DrawCentered(canvas, name, HorizontalCenter, NameY, MaxTextWidth, 1);
            }
        }
        else if (timers.Count > 1)
        {
            var n = Math.Min(timers.Count, 3);
            var scale = 1f / n;

            using var linePaint = new SKPaint()
            {
                Color = this.nameTextSettings.Paint.Color,
                StrokeWidth = 1,
            };
            for (int i = 1; i < n; i++)
            {
                canvas.DrawLine(Margin, Height * scale * i, Width - Margin, Height * scale * i, linePaint);
            }

            for (int i = 0; i < n; i++)
            {
                var name = timers[i].Name;
                var remaining = FormatTimer(timers[i].RemainingTime);
                var timeY = Height * scale * (i + TimeYRelative);
                this.timeTextSettings.DrawCentered(canvas, remaining, HorizontalCenter, timeY, MaxTextWidth, 1, true);
                if (!string.IsNullOrEmpty(name))
                {
                    var nameY = Height * scale * (i + NameYRelative);
                    this.nameTextSettings.DrawCentered(canvas, name, HorizontalCenter, nameY, MaxTextWidth, scale);
                }
            }
        }

        return surface.Snapshot();
    }

    private void DrawBackground(SKCanvas canvas)
    {
        if (this.settings.BackgroundImagePath is not null)
        {
            using var backgroundImage = SKBitmap.Decode(this.settings.BackgroundImagePath);
            if (backgroundImage is not null)
            {
                canvas.DrawBitmap(backgroundImage, new SKRect(0, 0, Width, Height));
                return;
            }
        }

        using var gradientShader = SKShader.CreateLinearGradient(
                        new SKPoint(0, 0),
                        new SKPoint(0, Height),
                        new[] { this.gradientStartColor, this.gradientEndColor },
                        null,
                        SKShaderTileMode.Clamp);
        using var gradientPaint = new SKPaint() { Shader = gradientShader };
        canvas.DrawRect(0, 0, Width, Height, gradientPaint);
    }

    private readonly struct TextSettings : IDisposable
    {
        private readonly float originalFontSize;

        public TextSettings(string? fontPath, string? fontFamily, float fontSize, string color)
        {
            this.originalFontSize = fontSize;
            this.Typeface = fontPath is not null
                ? SKTypeface.FromFile(fontPath)
                : SKTypeface.FromFamilyName(fontFamily ?? "Arial");

            this.Paint = new SKPaint
            {
                Color = SKColor.Parse(color),
                IsAntialias = true,
            };
        }

        public SKTypeface Typeface { get; init; }

        public SKPaint Paint { get; init; }

        public void Dispose()
        {
            this.Typeface.Dispose();
            this.Paint.Dispose();
        }

        public void DrawCentered(SKCanvas canvas, string text, float x, float y, int maxWidth, float scale = 1f, bool autoScale = false)
        {
            using var font = new SKFont(this.Typeface, this.originalFontSize * scale)
            {
                Subpixel = true,
            };

            this.DrawCentered(canvas, text, x, y, maxWidth, font, autoScale);
        }

        private void DrawCentered(SKCanvas canvas, string text, float x, float y, int maxWidth, SKFont font, bool autoScale)
        {
            font.MeasureText(text, out var bounds, this.Paint);
            if (bounds.Width < maxWidth)
            {
                canvas.DrawText(text, x, y + (bounds.Height / 2), SKTextAlign.Center, font, this.Paint);
            }
            else if (autoScale)
            {
                do
                {
                    font.Size *= 0.9f;
                    font.MeasureText(text, out bounds, this.Paint);
                }
                while (bounds.Width > maxWidth);

                canvas.DrawText(text, x, y + (bounds.Height / 2), SKTextAlign.Center, font, this.Paint);
            }
            else if (text.Length > 1)
            {
                if (autoScale)
                {
                    var newScale = (maxWidth - 2) / bounds.Width;
                    using var scaledFont = new SKFont(this.Typeface, font.Size * newScale)
                    {
                        Subpixel = true,
                    };

                    this.DrawCentered(canvas, text, x, y, maxWidth, scaledFont, true);
                }
                else
                {
                    this.DrawCentered(canvas, text.TrimEnd('…')[..^1] + '…', x, y, maxWidth, font, autoScale);
                }
            }
        }
    }
}
