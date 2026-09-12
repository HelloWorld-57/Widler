using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PostsService.Application.Messaging;
using PostsService.Infrastructure.Common;
using PostsService.Infrastructure.Db.Extensions;
using PostsService.Infrastructure.Db.Inbox;
using PostsService.Infrastructure.Messaging.Kafka.Dlq;
using PostsService.Infrastructure.Messaging.Kafka.DlqProducer;
using PostsService.Infrastructure.Messaging.Kafka.Processing;
using PostsService.Infrastructure.Messaging.Kafka.Producer;
using PostsService.Infrastructure.Observability;
using PostsService.Infrastructure.Telemetry;
using Serilog.Context;
using System.Diagnostics;
using System.Text;

namespace PostsService.Infrastructure.Messaging.Kafka.Consumer
{
    public class KafkaConsumerHostedService : BackgroundService
    {
        private readonly IServiceProvider _services;
        private IConsumer<string, string>? _consumer;
        private readonly IDlqProducer _dlq;
        private readonly KafkaTopics _topics;
        private readonly ILogger<KafkaConsumerHostedService> _logger;
        private readonly IConfiguration _config;

        public KafkaConsumerHostedService(
            IServiceProvider services,
            IConfiguration config,
            IDlqProducer dlq,
            IOptions<KafkaTopics> topics,
            ILogger<KafkaConsumerHostedService> logger)
        {
            _services = services;
            _dlq = dlq;
            _topics = topics.Value;
            _logger = logger;
            _config = config;
        }

        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = _config["Kafka:BootstrapServers"],
                GroupId = _config["Kafka:GroupId"],
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false
            };

            _consumer = new ConsumerBuilder<string, string>(consumerConfig)
                .SetErrorHandler((_, e) => _logger.LogError("Kafka error: {Reason}", e.Reason))
                .Build();


            var topics = new List<string>() {
                    _topics.Users
                };

            _consumer.Subscribe(topics);

            _logger.LogInformation("Kafka consumer subscribed to: {SubscribedTopics}", string.Join(", ", topics));

            try
            {
                while (!ct.IsCancellationRequested)
                {
                    try
                    {
                        var result = _consumer.Consume(ct);

                        var parentContext = ExtractActivityContext(result.Message.Headers);

                        using var activity = Tracing.ActivitySource.StartActivity(
                            "Kafka.Consume",
                            ActivityKind.Consumer,
                            parentContext ?? default);

                        activity?.SetTag("messaging.system", "kafka");
                        activity?.SetTag("messaging.destination", result.Topic);
                        activity?.SetTag("messaging.kafka.partition", result.Partition.Value);
                        activity?.SetTag("messaging.kafka.offset", result.Offset.Value);

                        using var logScope = CreateLogScope(result);

                        using var scope = _services.CreateScope();
                        var inbox = scope.ServiceProvider.GetRequiredService<IInboxService>();

                        var messageId = BuildMessageId(result);

                        if (await inbox.IsProcessedAsync(messageId, ct))
                        {
                            _consumer.Commit(result);
                            continue;
                        }

                        var headers = result.Message.Headers;

                        if (!headers.TryGetLastBytes("event-type", out var rawBytes))
                        {
                            throw new InvalidOperationException("Missing event-type header");
                        }

                        var eventType = Encoding.UTF8.GetString(rawBytes);

                        var processor = scope.ServiceProvider.GetRequiredService<IKafkaMessageProcessor>();

                        var sw = Stopwatch.StartNew();

                        try
                        {
                            await processor.ProcessAsync(eventType, result.Message.Value, ct);

                            await inbox.MarkProcessedAsync(
                                messageId,
                                result.Topic,
                                result.Partition.Value,
                                result.Offset.Value,
                                ct);

                            _consumer.Commit(result);
                        }
                        catch (DbUpdateException ex) when (ex.IsUniqueViolation())
                        {
                            // сообщение уже было обработано
                            _logger.LogWarning("Duplicate message skipped. MessageId={MessageId}", messageId);

                            _consumer.Commit(result);
                            continue;
                        }
                        catch (Exception ex)
                        {
                            var traceId = Activity.Current?.TraceId.ToString();

                            await _dlq.PublishAsync(
                                new DlqMessage(
                                    OriginalTopic: result.Topic,
                                    Partition: result.Partition.Value,
                                    Offset: result.Offset.Value,
                                    Key: result.Message.Key,
                                    Payload: result.Message.Value,
                                    Error: ex.ToString(),
                                    FailedAt: DateTime.UtcNow,
                                    TraceId: traceId),
                                ct);

                            _consumer.Commit(result);
                        }
                        finally
                        {
                            sw.Stop();
                            Metrics.OutboxProcessingDuration.Record(sw.Elapsed.TotalMilliseconds);
                        }
                    }
                    catch (ConsumeException ex) when (ex.Error.Code == ErrorCode.UnknownTopicOrPart)
                    {
                        _logger.LogWarning("Topic not ready, retrying in 5s...");
                        await Task.Delay(5000, ct);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Kafka consumer stopping...");
            }
            finally
            {
                _consumer.Close();
                _consumer?.Dispose();
            }
        }

        private static IDisposable CreateLogScope(ConsumeResult<string, string> cr)
        {
            var traceId = Activity.Current?.TraceId.ToString();
            var correlationId = ExtractCorrelationId(cr.Message.Headers);

            return new CompositeDisposable(
                LogContext.PushProperty("TraceId", traceId),
                LogContext.PushProperty("CorrelationId", correlationId));
        }

        private static ActivityContext? ExtractActivityContext(Headers headers)
        {
            if (!headers.TryGetLastBytes("traceparent", out var traceParentBytes))
            {
                return null;
            }

            var traceParent = Encoding.UTF8.GetString(traceParentBytes);

            string? traceState = null;

            if (headers.TryGetLastBytes("tracestate", out var traceStateBytes))
            {
                traceState = Encoding.UTF8.GetString(traceStateBytes);
            }

            if (ActivityContext.TryParse(
                    traceParent,
                    traceState,
                    out var context))
            {
                return context;
            }
            
            return null;
        }

        private static string? ExtractCorrelationId(Headers headers)
        {
            if (!headers.TryGetLastBytes(CorrelationHeaders.CorrelationId, out var correlationIdBytes))
            {
                return null;
            }

            return Encoding.UTF8.GetString(correlationIdBytes);
        }

        private static string BuildMessageId(ConsumeResult<string, string> cr)
            => $"{cr.Topic}:{cr.Partition}:{cr.Offset}";

    }
}