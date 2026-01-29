// <copyright file="TimerController.cs" company="Daniel Dreibrodt">
// Copyright (c) Daniel Dreibrodt. All rights reserved.
// </copyright>

namespace SmallTvTimerScreen.Controllers;

using System.Net.Mime;

using Microsoft.AspNetCore.Mvc;

using SmallTvTimerScreen.Data;
using SmallTvTimerScreen.Services;

/// <summary>
/// API controller for managing Alexa timers.
/// </summary>
/// <seealso cref="ControllerBase" />
[ApiController]
[Route("[controller]")]
public class TimerController : ControllerBase
{
    private readonly ITimerService timerService;
    private readonly ITimerImageGenerator timerImageGenerator;

    /// <summary>
    /// Initializes a new instance of the <see cref="TimerController"/> class.
    /// </summary>
    /// <param name="timerService">The timer service.</param>
    /// <param name="timerImageGenerator">The timer image generator.</param>
    public TimerController(ITimerService timerService, ITimerImageGenerator timerImageGenerator)
    {
        this.timerService = timerService;
        this.timerImageGenerator = timerImageGenerator;
    }

    /// <summary>
    /// Updates the active timers from a list of Alexa timers.
    /// </summary>
    /// <param name="value">The timer information.</param>
    /// <returns>
    /// The result.
    /// </returns>
    [HttpPost(nameof(NextTimer))]
    public IActionResult NextTimer([FromBody] NextTimerAttributes value)
    {
        var referenceTime = value.ProcessTimestamp ?? DateTimeOffset.Now;
        var activeTimers = (value.AlarmsBrief?.Active ?? [])
            .OrderBy(t => t.RemainingTime)
            .Select((t, i) => new NamedTimer
            {
                Name = t.Label,
                End = referenceTime.Add(TimeSpan.FromMilliseconds(t.RemainingTime)),
            })
            .ToList();
        if (activeTimers.Count > 0)
        {
            this.timerService.SetTimers(activeTimers);
        }
        else
        {
            this.timerService.ClearTimers();
        }

        return this.Ok();
    }

#if DEBUG

    /// <summary>
    /// Updates the active timers to a single timer.
    /// </summary>
    /// <param name="timer">The timer.</param>
    /// <returns>The result.</returns>
    [HttpPost(nameof(Update))]
    public IActionResult Update([FromBody] NamedTimer timer)
    {
        if (timer is null || timer.End < DateTimeOffset.Now)
        {
            this.timerService.ClearTimers();
        }
        else
        {
            this.timerService.SetTimers([timer]);
        }

        return this.Ok();
    }

    /// <summary>
    /// Gets a rendered image of <paramref name="n"/> timers with random remaining times.
    /// </summary>
    /// <param name="n">The number of timers (defaults to 1).</param>
    /// <returns>The genereated timer image.</returns>
    [HttpGet(nameof(Test))]
    public IActionResult Test([FromQuery] int n = 1)
    {
        var now = DateTimeOffset.Now;
        var random = new Random();
        var timers = Enumerable.Range(1, n).Select(i => new NamedTimer
        {
            Name = $"Timer #{i}",
            End = now + TimeSpan.FromMinutes(random.NextDouble() * 90),
        }).ToList();

        var jpeg = this.timerImageGenerator.CreateTimerImageAsJpeg(timers);
        return this.File(jpeg, MediaTypeNames.Image.Jpeg);
    }

#endif
}
