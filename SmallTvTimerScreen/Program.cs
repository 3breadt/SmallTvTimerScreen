// <copyright file="Program.cs" company="Daniel Dreibrodt">
// Copyright (c) Daniel Dreibrodt. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.HttpOverrides;

using SmallTvTimerScreen;

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<ImageSettings>(
    builder.Configuration.GetSection(ImageSettings.ConfigurationSection));
builder.Services.AddSingleton<TimerImageGenerator>();

builder.Services.AddSingleton<SmallTvService>();
builder.Services.AddHttpClient<SmallTvService>(httpClient =>
{
    httpClient.BaseAddress = new Uri(
        builder.Configuration.GetValue<string>(SmallTvService.SmallTvBaseUrl)
        ?? throw new InvalidOperationException("The SmallTV base URL is not configured."));
});

builder.Services.AddSingleton<TimerService>();
builder.Services.AddSingleton<ITimerService>(sp => sp.GetRequiredService<TimerService>());
builder.Services.AddHostedService(sp => sp.GetRequiredService<TimerService>());

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
