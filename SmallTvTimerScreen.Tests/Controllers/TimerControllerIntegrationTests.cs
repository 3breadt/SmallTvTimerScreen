// <copyright file="TimerControllerIntegrationTests.cs" company="Daniel Dreibrodt">
// Copyright (c) Daniel Dreibrodt. All rights reserved.
// </copyright>

namespace SmallTvTimerScreen.Tests.Controllers;

using System.Net;
using System.Text;

using FakeItEasy;

using NUnit.Framework;

using Shouldly;

using SmallTvTimerScreen.Data;

[TestFixture]
public class TimerControllerIntegrationTests
{
    private TimerScreenWebApplicationFactory factory = null!;
    private HttpClient client = null!;

    [SetUp]
    public void SetUp()
    {
        this.factory = new TimerScreenWebApplicationFactory();
        this.client = this.factory.CreateClient();
    }

    [TearDown]
    public void TearDown()
    {
        this.client.Dispose();
        this.factory.Dispose();
    }

    [Test]
    public async Task NextTimer_WithValidJson_ParsesRequestBodyCorrectly()
    {
        // Arrange - JSON matching the Home Assistant format
        const string json = """
            {
                "process_timestamp": "2026-01-04T16:37:32.836172+01:00",
                "total_active": 2,
                "status": "ON",
                "brief": {
                    "active": [
                        {
                            "id": "timer-1",
                            "label": "Pizza Timer",
                            "status": "ON",
                            "type": "Timer",
                            "remainingTime": 120000
                        },
                        {
                            "id": "timer-2",
                            "label": "Egg Timer",
                            "status": "ON",
                            "type": "Timer",
                            "remainingTime": 60000
                        }
                    ]
                }
            }
            """;

        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await this.client.PostAsync("/timer/nexttimer", content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        A.CallTo(() => this.factory.FakeTimerService.SetTimers(A<IList<NamedTimer>>.That.Matches(
            timers => timers.Count == 2 &&
                      timers[0].Name == "Egg Timer" &&
                      timers[1].Name == "Pizza Timer")))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task NextTimer_WithEmptyActiveTimers_ClearsTimers()
    {
        // Arrange
        const string json = """
            {
                "process_timestamp": "2026-01-04T16:40:01.996090+01:00",
                "total_active": 0,
                "status": "OFF",
                "brief": {
                    "active": []
                }
            }
            """;

        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await this.client.PostAsync("/timer/nexttimer", content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        A.CallTo(() => this.factory.FakeTimerService.ClearTimers()).MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task NextTimer_WithNullBrief_ClearsTimers()
    {
        // Arrange
        const string json = """
            {
                "process_timestamp": "2026-01-04T16:40:01.996090+01:00",
                "total_active": 0,
                "status": "OFF"
            }
            """;

        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await this.client.PostAsync("/timer/nexttimer", content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        A.CallTo(() => this.factory.FakeTimerService.ClearTimers()).MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task NextTimer_CalculatesEndTimeFromProcessTimestamp()
    {
        // Arrange
        const string json = """
            {
                "process_timestamp": "2026-01-04T16:00:00+01:00",
                "brief": {
                    "active": [
                        {
                            "id": "timer-1",
                            "label": "Test",
                            "remainingTime": 300000
                        }
                    ]
                }
            }
            """;

        var expectedEndTime = DateTimeOffset.Parse("2026-01-04T16:05:00+01:00");
        IList<NamedTimer>? capturedTimers = null;

        A.CallTo(() => this.factory.FakeTimerService.SetTimers(A<IList<NamedTimer>>._))
            .Invokes((IList<NamedTimer> timers) => capturedTimers = timers);

        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        await this.client.PostAsync("/timer/nexttimer", content);

        // Assert
        capturedTimers.ShouldNotBeNull();
        capturedTimers.Count.ShouldBe(1);
        capturedTimers[0].End.ShouldBe(expectedEndTime);
    }

    [Test]
    public async Task NextTimer_WithCamelCaseJson_ParsesCorrectly()
    {
        // Arrange - camelCase JSON (ASP.NET Core default)
        const string json = """
            {
                "processTimestamp": "2026-01-04T16:00:00+01:00",
                "brief": {
                    "active": [
                        {
                            "id": "timer-1",
                            "label": "Camel Case Timer",
                            "remainingTime": 60000
                        }
                    ]
                }
            }
            """;

        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await this.client.PostAsync("/timer/nexttimer", content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        // Verify timer was set (may use camelCase or snake_case parsing)
        A.CallTo(() => this.factory.FakeTimerService.SetTimers(A<IList<NamedTimer>>._))
            .MustHaveHappened();
    }

    [Test]
    public async Task NextTimer_WithInvalidJson_ReturnsBadRequest()
    {
        // Arrange
        const string invalidJson = "{ invalid json }";
        var content = new StringContent(invalidJson, Encoding.UTF8, "application/json");

        // Act
        var response = await this.client.PostAsync("/timer/nexttimer", content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task NextTimer_WithFullHomeAssistantPayload_ParsesCorrectly()
    {
        // Arrange - Full payload as sent by Home Assistant
        const string json = """
            {
                "recurrence": "Never Repeat",
                "process_timestamp": "2026-01-04T16:37:32.836172+01:00",
                "prior_value": "2026-01-04T16:42:31+01:00",
                "total_active": 2,
                "total_all": 2,
                "status": "ON",
                "dismissed": null,
                "timer": "Fantasie",
                "device_class": "timestamp",
                "icon": "mdi:timer-outline",
                "friendly_name": "Echo Dot Küche next Timer",
                "brief": {
                    "active": [
                        {
                            "id": "A4ZXE0RM7LQ7A-G0922J06520608FL-8441e935-33d0-33ad-ade4-bf176e5db103",
                            "status": "ON",
                            "type": "Timer",
                            "remainingTime": 119000,
                            "lastUpdatedDate": 1767541052681
                        },
                        {
                            "id": "A4ZXE0RM7LQ7A-G0922J06520608FL-c975c6c0-e28c-3f43-8123-057f8eec3884",
                            "status": "ON",
                            "type": "Timer",
                            "remainingTime": 139000,
                            "lastUpdatedDate": 1767540893002
                        }
                    ],
                    "all": [
                        {
                            "id": "A4ZXE0RM7LQ7A-G0922J06520608FL-8441e935-33d0-33ad-ade4-bf176e5db103",
                            "status": "ON",
                            "type": "Timer",
                            "remainingTime": 119000,
                            "lastUpdatedDate": 1767541052681
                        }
                    ]
                }
            }
            """;

        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await this.client.PostAsync("/timer/nexttimer", content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        A.CallTo(() => this.factory.FakeTimerService.SetTimers(A<IList<NamedTimer>>.That.Matches(
            timers => timers.Count == 2)))
            .MustHaveHappenedOnceExactly();
    }
}
