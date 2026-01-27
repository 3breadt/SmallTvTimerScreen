// <copyright file="TimerScreenWebApplicationFactory.cs" company="Daniel Dreibrodt">
// Copyright (c) Daniel Dreibrodt. All rights reserved.
// </copyright>

namespace SmallTvTimerScreen.Tests;

using FakeItEasy;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

using SmallTvTimerScreen;
using SmallTvTimerScreen.Services; 

public class TimerScreenWebApplicationFactory : WebApplicationFactory<Program>
{
    public ITimerService FakeTimerService { get; } = A.Fake<ITimerService>();

    public ISmallTvService FakeSmallTvService { get; } = A.Fake<ISmallTvService>();

    public ITimerImageGenerator FakeTimerImageGenerator { get; } = A.Fake<ITimerImageGenerator>();

    /// <inheritdoc/>
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IHostedService>();
            services.RemoveAll<ITimerService>();
            services.RemoveAll<ITimerImageGenerator>();
            services.RemoveAll<ISmallTvService>();

            services.AddSingleton(this.FakeTimerService);
            services.AddSingleton(this.FakeTimerImageGenerator);
            services.AddSingleton(this.FakeSmallTvService);
        });

        builder.UseEnvironment("Testing");
    }
}
