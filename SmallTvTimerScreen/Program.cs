// <copyright file="Program.cs" company="Daniel Dreibrodt">
// Copyright (c) Daniel Dreibrodt. All rights reserved.
// </copyright>

namespace SmallTvTimerScreen;

using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.HttpOverrides;

using SmallTvTimerScreen.Services;

/// <summary>
/// Entry point for the application.
/// </summary>
public partial class Program
{
    /// <summary>
    /// Defines the entry point of the application.
    /// </summary>
    /// <param name="args">The arguments.</param>
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.Configure<ImageSettings>(
            builder.Configuration.GetSection(ImageSettings.ConfigurationSection));
        builder.Services.AddSingleton<ITimerImageGenerator, TimerImageGenerator>();

        builder.Services.AddSingleton<ISmallTvService, SmallTvService>();
        builder.Services.AddHttpClient<SmallTvService>(httpClient =>
        {
            httpClient.BaseAddress = new Uri(
                builder.Configuration.GetValue<string>(SmallTvService.SmallTvBaseUrl)
                ?? throw new InvalidOperationException("The SmallTV base URL is not configured."));
        });

        builder.Services.AddSingleton<ITimerService, TimerService>();
        builder.Services.AddHostedService(sp => (TimerService)sp.GetRequiredService<ITimerService>());

        builder.Services.AddControllers();

        builder.Services.AddHttpLogging(logging =>
        {
            logging.LoggingFields = HttpLoggingFields.All;
            logging.MediaTypeOptions.AddText("application/json");
            logging.RequestBodyLogLimit = 4096;
            logging.ResponseBodyLogLimit = 4096;
            logging.CombineLogs = true;
        });

        var app = builder.Build();

        if (!app.Environment.IsDevelopment())
        {
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
            });
        }

        app.UseAuthorization();
        app.UseHttpLogging();
        app.MapControllers();

        app.Run();
    }
}