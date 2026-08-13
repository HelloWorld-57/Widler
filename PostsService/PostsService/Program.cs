using Asp.Versioning;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using PostsService.Application.IntegrationEvents.ProcessManagers;
using PostsService.Application.Interfaces;
using PostsService.Application.Messaging;
using PostsService.Application.Services;
using PostsService.Application.Validation;
using PostsService.Infrastructure.Db;
using PostsService.Infrastructure.Db.Inbox;
using PostsService.Infrastructure.Db.Outbox;
using PostsService.Infrastructure.Events.Kafka.Messages.Users.v1;
using PostsService.Infrastructure.Messaging.Kafka;
using PostsService.Infrastructure.Messaging.Kafka.Consumer;
using PostsService.Infrastructure.Messaging.Kafka.Dlq;
using PostsService.Infrastructure.Messaging.Kafka.DlqProducer;
using PostsService.Infrastructure.Messaging.Kafka.Processing;
using PostsService.Infrastructure.Messaging.Kafka.Producer;
using PostsService.Infrastructure.Messaging.Kafka.Routing;
using PostsService.Infrastructure.Middleware;
using PostsService.Infrastructure.Telemetry;
using Serilog;
using Serilog.Context;
using Serilog.Formatting.Json;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration) 
    .Enrich.FromLogContext()
    .Enrich.WithProperty("service.name", "PostsService")
    .WriteTo.Console(new JsonFormatter()) 
    .CreateLogger();

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing
            .SetResourceBuilder(
                ResourceBuilder.CreateDefault()
                    .AddService("PostsService")
            )
            .AddSource(Tracing.SourceName)
            .AddAspNetCoreInstrumentation(options =>
            {
                options.RecordException = true;
            })
            .AddHttpClientInstrumentation()
            .AddEntityFrameworkCoreInstrumentation(options =>
            {
                options.EnrichWithIDbCommand = (activity, command) =>
                {
                    activity.SetTag("db.statement", command.CommandText);
                };
            })
            .AddOtlpExporter(options =>
            {
                options.Endpoint = new Uri("http://jaeger:4317");
            });
    })
    .WithMetrics(metrics =>
    {
        metrics
            .AddMeter("PostsService")
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddRuntimeInstrumentation()
            .AddPrometheusExporter();
    });

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;

    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader()   // /v1/...
    );
});

builder.Host.UseSerilog();

builder.Services.AddScoped<OutboxSaveChangesInterceptor>();
builder.Services.AddDbContext<PostsDbContext>((sp, options) =>
{
    var interceptor = sp.GetRequiredService<OutboxSaveChangesInterceptor>();
    options.AddInterceptors(interceptor);
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
            .UseSnakeCaseNamingConvention();
});

builder.Services.Configure<KafkaTopics>(builder.Configuration.GetSection("Kafka:Topics"));
builder.Services.AddSingleton<IKafkaProducerService, KafkaProducerService>();
builder.Services.AddHostedService<OutboxProcessor>();

builder.Services.AddScoped<IInboxService, InboxService>();

if (!builder.Environment.IsEnvironment("Migration"))
{
    builder.Services.AddHostedService<KafkaConsumerHostedService>();
}

builder.Services.AddScoped<IKafkaMessageProcessor, KafkaMessageProcessor>();
builder.Services.AddSingleton<IDlqProducer, KafkaDlqProducer>();

builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<IPostRepository, PostRepository>();

builder.Services.AddScoped<IIntegrationEventHandler<UserDeletedV1>,UserDeletedProcessManager>();
builder.Services.AddSingleton<IIntegrationEventRouter, IntegrationEventRouter>();

builder.Services.AddValidatorsFromAssemblyContaining<CreatePostCommandValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<UpdatePostCommandValidator>();
builder.Services.AddScoped<IValidatorRunner, ValidatorRunner>();

builder.Services.AddControllers();

builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactViteFrontend", policy =>
    {
        policy.WithOrigins(
            "http://localhost:5173"//,
            //"https://myapp.example.com"
            )
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseSerilogRequestLogging();

app.MapPrometheusScrapingEndpoint();

app.Use(async (context, next) =>
{
    var traceId =
        Activity.Current?.TraceId.ToString()
        ?? context.TraceIdentifier;

    using (LogContext.PushProperty("TraceId", traceId))
    {
        await next();
    }
});

if (args.Contains("--migrate"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<PostsDbContext>();
    db.Database.Migrate();
    Console.WriteLine("Migrations applied successfully.");
    return;
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "v1");
    });
}

app.UseCors("AllowReactViteFrontend");

//app.UseHttpsRedirection();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.MapGet("/health", () => Results.Ok("OK"));

app.Run();
