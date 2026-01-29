// <copyright file="TimerService.cs" company="Daniel Dreibrodt">
// Copyright (c) Daniel Dreibrodt. All rights reserved.
// </copyright>

namespace SmallTvTimerScreen.Services;

using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using SmallTvAlexaTimer;

using SmallTvTimerScreen.Data;

/// <summary>
/// Manages the updating and displaying of timer images on the SmallTV device.
/// </summary>
/// <seealso cref="BackgroundService" />
public sealed class TimerService : BackgroundService, ITimerService
{
    private const string FileName = "timer.gif";

    private readonly ITimerImageGenerator imageGenerator;
    private readonly ISmallTvService smallTv;
    private readonly ILogger logger;
    private readonly SemaphoreSlim timerSetSignal = new(0, 1);
    private volatile List<NamedTimer> activeTimers = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="TimerService"/> class.
    /// </summary>
    /// <param name="imageGenerator">The image generator.</param>
    /// <param name="smallTv">The small tv.</param>
    /// <param name="logger">The logger.</param>
    public TimerService(ITimerImageGenerator imageGenerator, ISmallTvService smallTv, ILogger<TimerService> logger)
    {
        this.imageGenerator = imageGenerator;
        this.smallTv = smallTv;
        this.logger = logger;
    }

    /// <summary>
    /// Gets a value indicating whether a timer is currently active.
    /// </summary>
    public bool IsTimerActive => this.activeTimers.Any(t => t.IsActive);

    /// <summary>
    /// Clears the active timers.
    /// </summary>
    public void ClearTimers() => this.SetTimers([]);

    /// <summary>
    /// Sets the active timers.
    /// </summary>
    /// <param name="timers">The timers.</param>
    public void SetTimers(IList<NamedTimer> timers)
    {
        List<NamedTimer> newValue = [.. timers.OrderBy(t => t.End).ThenBy(t => t.Name)];
        var previousValue = Interlocked.Exchange(ref this.activeTimers, newValue);
        if (!previousValue.Any(t => t.IsActive) && newValue.Any(t => t.IsActive))
        {
            this.timerSetSignal.Release();
        }
    }

    /// <inheritdoc/>
    public override void Dispose()
    {
        this.timerSetSignal.Dispose();
        base.Dispose();
    }

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await this.timerSetSignal.WaitAsync(stoppingToken);
                if (this.IsTimerActive)
                {
                    this.logger.LogTimerServiceNewTimerStarted(this.activeTimers.FirstOrDefault()?.Name ?? "Unnamed Timer");
                    await this.UpdateTimerImage(stoppingToken);
                    await this.smallTv.ShowImage(FileName, stoppingToken);
                    await this.smallTv.SwitchToPhotoAlbum(stoppingToken);

                    using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
                    while (this.IsTimerActive)
                    {
                        await timer.WaitForNextTickAsync(stoppingToken);
                        await this.UpdateTimerImage(stoppingToken);
                    }

                    await this.smallTv.SwitchToDefaultTheme(stoppingToken);
                    this.logger.LogTimerServiceTimersEnded();
                }
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            if (this.IsTimerActive)
            {
                this.logger.LogTimerServiceStoppedWhileTimerActive();
                await this.smallTv.SwitchToDefaultTheme(default);
            }
        }
    }

    private async Task UpdateTimerImage(CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();
        var jpeg = this.imageGenerator.CreateTimerImageAsGif(this.activeTimers);
        await this.smallTv.UploadImage(FileName, jpeg, cancellationToken);
        sw.Stop();
        this.logger.LogTimerServiceImageUpdateDuration(sw.ElapsedMilliseconds);
    }
}
