// <copyright file="SmallTvService.cs" company="Daniel Dreibrodt">
// Copyright (c) Daniel Dreibrodt. All rights reserved.
// </copyright>

namespace SmallTvTimerScreen;

using System.Net.Mime;
using System.Web;

using SmallTvAlexaTimer;

/// <summary>
/// Service for controlling a GeekMagic SmallTV device.
/// </summary>
public class SmallTvService
{
    /// <summary>
    /// Name of the configuration key for the SmallTV base URL.
    /// </summary>
    public const string SmallTvBaseUrl = nameof(SmallTvBaseUrl);

    /// <summary>
    /// Name of the configuration key for the SmallTV default theme ID.
    /// </summary>
    public const string SmallTvDefaultThemeId = nameof(SmallTvDefaultThemeId);

    private static readonly string[] ThemeNames = ["0 (Invalid)", "Weather Clock Today", "Weather Forecast", "Photo Album", "Time Style 1", "Time Style 2", "Time Style 3", "Simple Weather Clock"];

    private readonly int defaultThemeId;
    private readonly HttpClient httpClient;
    private readonly ILogger logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SmallTvService" /> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client.</param>
    /// <param name="config">The configuration.</param>
    /// <param name="logger">The logger.</param>
    public SmallTvService(HttpClient httpClient, IConfiguration config, ILogger<SmallTvService> logger)
    {
        this.httpClient = httpClient;
        this.defaultThemeId = config.GetValue(SmallTvDefaultThemeId, 1);
        this.logger = logger;
    }

    /// <summary>
    /// Switches the TV to the default theme.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task SwitchToDefaultTheme(CancellationToken cancellationToken)
    {
        try
        {
            await this.httpClient.GetAsync("/set?theme=" + this.defaultThemeId, cancellationToken);
        }
        catch (Exception ex)
        {
            this.logger.LogSmallTvApiSetThemeFailed(ex, ThemeNames.ElementAtOrDefault(this.defaultThemeId) ?? $"{this.defaultThemeId} (Unknown)");
        }
    }

    /// <summary>
    /// Switches the TV to the photo album theme.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task SwitchToPhotoAlbum(CancellationToken cancellationToken)
    {
        try
        {
            await this.httpClient.GetAsync("/set?theme=3", cancellationToken);
        }
        catch (Exception ex)
        {
            this.logger.LogSmallTvApiSetThemeFailed(ex, "Photo Album");
        }
    }

    /// <summary>
    /// Uploads an image to the photo album.
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    /// <param name="data">The data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task UploadImage(string fileName, byte[] data, CancellationToken cancellationToken)
    {
        try
        {
            var file = new ByteArrayContent(data)
            {
                Headers =
                {
                    ContentDisposition = new("form-data")
                    {
                        Name = "file",
                        FileName = '"' + fileName + '"',
                    },
                    ContentType = new(Path.GetExtension(fileName)?.ToLowerInvariant() == ".jpg" ? MediaTypeNames.Image.Jpeg : MediaTypeNames.Image.Gif),
                },
            };

            var content = new MultipartFormDataContent() { file };

            await this.httpClient.PostAsync("/doUpload?dir=" + HttpUtility.UrlEncode("/image/"), content, cancellationToken);
        }
        catch (Exception ex)
        {
            this.logger.LogSmallTvApiUploadImageFailed(ex, fileName);
        }
    }

    /// <summary>
    /// Shows an image from the photo album.
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task ShowImage(string fileName, CancellationToken cancellationToken)
    {
        try
        {
            await this.httpClient.GetAsync("/set?img=" + HttpUtility.UrlEncode("/image/" + fileName), cancellationToken);
        }
        catch (Exception ex)
        {
            this.logger.LogSmallTvApiSetImageFailed(ex, fileName);
        }
    }

    /// <summary>
    /// Deletes an image from the photo album.
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task DeleteImage(string fileName, CancellationToken cancellationToken)
    {
        try
        {
            await this.httpClient.GetAsync("/delete?file=" + HttpUtility.UrlEncode("/image/" + fileName), cancellationToken);
        }
        catch (Exception ex)
        {
            this.logger.LogSmallTvApiDeleteImageFailed(ex, fileName);
        }
    }

    /// <summary>
    /// Deletes all images from the photo album.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task DeleteAllImages(CancellationToken cancellationToken)
    {
        try
        {
            await this.httpClient.GetAsync("/set?clear=image", cancellationToken);
        }
        catch (Exception ex)
        {
            this.logger.LogSmallTvApiClearPhotoAlbumFailed(ex);
        }
    }
}
