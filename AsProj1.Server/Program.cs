using Microsoft.AspNetCore.Mvc;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.Metrics;
using OpenTelemetry;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers().AddNewtonsoftJson();


builder.Services.AddOpenTelemetry().ConfigureResource(resource => resource
    .AddService(serviceName: builder.Environment.ApplicationName));

builder.Services.AddOpenTelemetry().WithTracing((builder) =>
{
    builder.AddAspNetCoreInstrumentation();
   // builder.AddConsoleExporter();
    builder.AddOtlpExporter();
});

builder.Logging.AddOpenTelemetry(x =>
{
    //x.AddOtlpExporter();
    x.AddConsoleExporter();
});


builder.Services.AddOpenTelemetry().WithMetrics(builder =>
{
    builder
    .AddMeter("LOGINNN")
    .AddRuntimeInstrumentation()
    .AddPrometheusExporter();
    //.AddConsoleExporter();
});


var app = builder.Build();


app.UseOpenTelemetryPrometheusScrapingEndpoint();
app.UseDefaultFiles();
app.UseStaticFiles();

// Configuração CORS
app.UseCors(builder => builder
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapFallbackToFile("/index.html");
app.Run();
