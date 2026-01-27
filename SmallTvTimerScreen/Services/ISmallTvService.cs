// <copyright file="ISmallTvService.cs" company="Daniel Dreibrodt">
// Copyright (c) Daniel Dreibrodt. All rights reserved.
// </copyright>

namespace SmallTvTimerScreen.Services
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Interface for a service that controls a SmallTV device.
    /// </summary>
    public interface ISmallTvService
    {
        /// <summary>
        /// Deletes all images from the photo album.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task DeleteAllImages(CancellationToken cancellationToken);

        /// <summary>
        /// Deletes an image from the photo album.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task DeleteImage(string fileName, CancellationToken cancellationToken);

        /// <summary>
        /// Shows an image from the photo album.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task ShowImage(string fileName, CancellationToken cancellationToken);

        /// <summary>
        /// Switches the TV to the default theme.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task SwitchToDefaultTheme(CancellationToken cancellationToken);

        /// <summary>
        /// Switches the TV to the photo album theme.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task SwitchToPhotoAlbum(CancellationToken cancellationToken);

        /// <summary>
        /// Uploads an image to the photo album.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="data">The data.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task UploadImage(string fileName, byte[] data, CancellationToken cancellationToken);
    }
}