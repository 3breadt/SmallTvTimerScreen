// <copyright file="TimerControllerTests.cs" company="Daniel Dreibrodt">
// Copyright (c) Daniel Dreibrodt. All rights reserved.
// </copyright>

namespace SmallTvTimerScreen.Tests.Controllers;

using FakeItEasy;

using Microsoft.AspNetCore.Mvc;

using NUnit.Framework;

using Shouldly;

using SmallTvTimerScreen.Controllers;
using SmallTvTimerScreen.Data;
using SmallTvTimerScreen.Services;

[TestFixture]
public class TimerControllerTests
{
    private ITimerService timerService = null!;
    private TimerController controller = null!;

    [SetUp]
    public void SetUp()
    {
        this.timerService = A.Fake<ITimerService>();
        this.controller = new TimerController(this.timerService, null!);
    }

    [Test]
    public void NextTimer_WithActiveTimers_ReturnsOkAndSetsTimers()
    {
        // Arrange
        var processTimestamp = DateTimeOffset.Parse("2026-01-04T16:37:32+01:00");
        var input = new NextTimerAttributes
        {
            ProcessTimestamp = processTimestamp,
            AlarmsBrief = new AlarmsBrief
            {
                Active =
                [
                    new AlexaTimer
                    {
                        Id = "timer-1",
                        Label = "Pizza",
                        Status = AlexaTimerStatus.On,
                        Type = AlexaTimerType.Timer,
                        RemainingTime = 120000, // 2 minutes
                    },
                    new AlexaTimer
                    {
                        Id = "timer-2",
                        Label = "Eggs",
                        Status = AlexaTimerStatus.On,
                        Type = AlexaTimerType.Timer,
                        RemainingTime = 60000, // 1 minute
                    },
                ],
            },
        };

        // Act
        var result = this.controller.NextTimer(input);

        // Assert
        result.ShouldBeOfType<OkResult>();
        A.CallTo(() => this.timerService.SetTimers(A<IList<NamedTimer>>.That.Matches(
            timers => timers.Count == 2 &&
                      timers[0].Name == "Eggs" && // Ordered by remaining time
                      timers[1].Name == "Pizza")))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public void NextTimer_WithNoActiveTimers_ReturnsOkAndClearsTimers()
    {
        // Arrange
        var input = new NextTimerAttributes
        {
            ProcessTimestamp = DateTimeOffset.Now,
            AlarmsBrief = new AlarmsBrief
            {
                Active = [],
            },
        };

        // Act
        var result = this.controller.NextTimer(input);

        // Assert
        result.ShouldBeOfType<OkResult>();
        A.CallTo(() => this.timerService.ClearTimers()).MustHaveHappenedOnceExactly();
        A.CallTo(() => this.timerService.SetTimers(A<IList<NamedTimer>>._)).MustNotHaveHappened();
    }

    [Test]
    public void NextTimer_WithNullAlarmsBrief_ReturnsOkAndClearsTimers()
    {
        // Arrange
        var input = new NextTimerAttributes
        {
            ProcessTimestamp = DateTimeOffset.Now,
            AlarmsBrief = null,
        };

        // Act
        var result = this.controller.NextTimer(input);

        // Assert
        result.ShouldBeOfType<OkResult>();
        A.CallTo(() => this.timerService.ClearTimers()).MustHaveHappenedOnceExactly();
    }

    [Test]
    public void NextTimer_CalculatesEndTimeCorrectly()
    {
        // Arrange
        var processTimestamp = DateTimeOffset.Parse("2026-01-04T16:00:00+01:00");
        var remainingTimeMs = 300000; // 5 minutes
        var expectedEndTime = processTimestamp.AddMilliseconds(remainingTimeMs);

        var input = new NextTimerAttributes
        {
            ProcessTimestamp = processTimestamp,
            AlarmsBrief = new AlarmsBrief
            {
                Active =
                [
                    new AlexaTimer
                    {
                        Id = "timer-1",
                        Label = "Test Timer",
                        RemainingTime = remainingTimeMs,
                    },
                ],
            },
        };

        IList<NamedTimer>? capturedTimers = null;
        A.CallTo(() => this.timerService.SetTimers(A<IList<NamedTimer>>._))
            .Invokes((IList<NamedTimer> timers) => capturedTimers = timers);

        // Act
        this.controller.NextTimer(input);

        // Assert
        capturedTimers.ShouldNotBeNull();
        capturedTimers.Count.ShouldBe(1);
        capturedTimers[0].End.ShouldBe(expectedEndTime);
    }

    [Test]
    public void NextTimer_OrdersTimersByRemainingTime()
    {
        // Arrange
        var input = new NextTimerAttributes
        {
            ProcessTimestamp = DateTimeOffset.Now,
            AlarmsBrief = new AlarmsBrief
            {
                Active =
                [
                    new AlexaTimer { Label = "Third", RemainingTime = 300000 },
                    new AlexaTimer { Label = "First", RemainingTime = 60000 },
                    new AlexaTimer { Label = "Second", RemainingTime = 120000 },
                ],
            },
        };

        IList<NamedTimer>? capturedTimers = null;
        A.CallTo(() => this.timerService.SetTimers(A<IList<NamedTimer>>._))
            .Invokes((IList<NamedTimer> timers) => capturedTimers = timers);

        // Act
        this.controller.NextTimer(input);

        // Assert
        capturedTimers.ShouldNotBeNull();
        capturedTimers[0].Name.ShouldBe("First");
        capturedTimers[1].Name.ShouldBe("Second");
        capturedTimers[2].Name.ShouldBe("Third");
    }
}
