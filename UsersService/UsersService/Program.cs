using Asp.Versioning;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Context;
using Serilog.Formatting.Json;
using System.Diagnostics;
using UsersService.Application.Interfaces;
using UsersService.Application.Messaging;
using UsersService.Application.Services;
using UsersService.Application.Validation;
using UsersService.Infrastructure.Db;
using UsersService.Infrastructure.Db.Inbox;
using UsersService.Infrastructure.Db.Outbox;
using UsersService.Infrastructure.Messaging.Kafka;
using UsersService.Infrastructure.Messaging.Kafka.Consumer;
using UsersService.Infrastructure.Messaging.Kafka.Dlq;
using UsersService.Infrastructure.Messaging.Kafka.DlqProducer;
using UsersService.Infrastructure.Messaging.Kafka.Processing;
using UsersService.Infrastructure.Messaging.Kafka.Producer;
using UsersService.Infrastructure.Messaging.Kafka.Routing;
using UsersService.Infrastructure.Middleware;
using UsersService.Infrastructure.Telemetry;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("service.name", "UsersService")
    .WriteTo.Console(new JsonFormatter())
    .CreateLogger();

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing
            .SetResourceBuilder(
                ResourceBuilder.CreateDefault()
                    .AddService("UsersService")
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
            .AddMeter("UsersService")
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
builder.Services.AddDbContext<UsersDbContext>((sp, options) =>
{
    var interceptor = sp.GetRequiredService<OutboxSaveChangesInterceptor>();
    options.AddInterceptors(interceptor);
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
           .UseSnakeCaseNamingConvention();
});

builder.Services.Configure<KafkaTopics>(builder.Configuration.GetSection("Kafka:Topics"));
builder.Services.AddSingleton<IKafkaProducerService, KafkaProducerService>();
builder.Services.AddHostedService<OutboxProcessor>();

//builder.Services.AddScoped<IInboxService, InboxService>();

//if (!builder.Environment.IsEnvironment("Migration"))
//{
//    builder.Services.AddHostedService<KafkaConsumerHostedService>();
//}

//builder.Services.AddScoped<IKafkaMessageProcessor, KafkaMessageProcessor>();
//builder.Services.AddSingleton<IDlqProducer, KafkaDlqProducer>();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

//builder.Services.AddScoped<IIntegrationEventHandler<UserDeletedV1>, UserDeletedProcessManager>();
//builder.Services.AddSingleton<IIntegrationEventRouter, IntegrationEventRouter>();

builder.Services.AddValidatorsFromAssemblyContaining<CreateUserCommandValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<UpdateUserCommandValidator>();
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
    var db = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
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
