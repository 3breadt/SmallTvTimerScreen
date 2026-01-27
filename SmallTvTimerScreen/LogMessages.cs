// <copyright file="LogMessages.cs" company="Daniel Dreibrodt">
// Copyright (c) Daniel Dreibrodt. All rights reserved.
// </copyright>

namespace SmallTvAlexaTimer;

/// <summary>
/// Predefined log messages for the application.
/// </summary>
internal static partial class LogMessages
{
    [LoggerMessage(Level = LogLevel.Error, Message = "Could not switch SmallTV theme to \"{ThemeName}\".")]
    public static partial void LogSmallTvApiSetThemeFailed(this ILogger logger, Exception ex, string themeName);

    [LoggerMessage(Level = LogLevel.Error, Message = "Could not upload image \"{FileName}\" to SmallTV.")]
    public static partial void LogSmallTvApiUploadImageFailed(this ILogger logger, Exception ex, string fileName);

    [LoggerMessage(Level = LogLevel.Error, Message = "Could not show image \"{FileName}\" on SmallTV.")]
    public static partial void LogSmallTvApiSetImageFailed(this ILogger logger, Exception ex, string fileName);

    [LoggerMessage(Level = LogLevel.Error, Message = "Could not delete image \"{FileName}\" on SmallTV.")]
    public static partial void LogSmallTvApiDeleteImageFailed(this ILogger logger, Exception ex, string fileName);

    [LoggerMessage(Level = LogLevel.Error, Message = "Could not clear photo album on SmallTV.")]
    public static partial void LogSmallTvApiClearPhotoAlbumFailed(this ILogger logger, Exception ex);

    [LoggerMessage(Level = LogLevel.Information, Message = "New timer started: {Name}")]
    public static partial void LogTimerServiceNewTimerStarted(this ILogger logger, string name);

    [LoggerMessage(Level = LogLevel.Information, Message = "Timer(s) have ended, switching to weather display.")]
    public static partial void LogTimerServiceTimersEnded(this ILogger logger);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Timer service was stopped while a timer was active. Switching to weather display.")]
    public static partial void LogTimerServiceStoppedWhileTimerActive(this ILogger logger);

    [LoggerMessage(Level = LogLevel.Trace, Message = "Took {Duration} ms to update the timer image.")]
    public static partial void LogTimerServiceImageUpdateDuration(this ILogger logger, long duration);
}