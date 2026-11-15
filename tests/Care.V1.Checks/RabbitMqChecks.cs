using System.Text.Json;
using foll_care.Care.Application.IntegrationEvents;
using foll_care.Care.Infrastructure.Messaging.Configuration;
using foll_care.Care.Infrastructure.Messaging.RabbitMq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

internal static class RabbitMqChecks
{
    public static async Task RunAsync(Action<bool, string> check)
    {
        var settings = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json").AddEnvironmentVariables().Build()
            .GetSection("RabbitMq").Get<RabbitMqOptions>()!;
        var suffix = Guid.NewGuid().ToString("N");
        settings.ExchangeName = "foll.checks.Care." + suffix;
        settings.QueueName = "foll.checks.Care." + suffix;
        settings.CompletionQueueNames = [settings.QueueName + ".iam", settings.QueueName + ".devices"];
        var keys = new[] { InvitationChangedIntegrationEvent.CreatedEventType, InvitationChangedIntegrationEvent.AcceptedEventType, InvitationChangedIntegrationEvent.RejectedEventType };
        var factory = new ConnectionFactory {
            HostName = settings.HostName, Port = settings.Port, UserName = settings.UserName,
            Password = settings.Password, VirtualHost = settings.VirtualHost,
            RequestedConnectionTimeout = TimeSpan.FromSeconds(5)
        };
        await using var admin = await factory.CreateConnectionAsync();
        await using var channel = await admin.CreateChannelAsync();
        await using var transport = new RabbitMqConnection(Options.Create(settings), keys,
            NullLogger<RabbitMqConnection>.Instance);
        var publisher = new RabbitMqIntegrationEventPublisher(transport);
        InvitationChangedIntegrationEvent Make(string kind) => new(Guid.NewGuid(), Guid.NewGuid(), 1, 1, "Paciente de prueba", 10, 20, 1, "Familiar", kind == "created" ? "Pending" : kind == "accepted" ? "Accepted" : "Rejected", DateTime.UtcNow.AddDays(2), kind, DateTime.UtcNow);
        try
        {
            var completion = new foll_care.Shared.Infrastructure.Messaging.AccountDeletionCompleted(
                Guid.NewGuid(), Guid.NewGuid(), 10, new long[] { 20, 30 }, DateTime.UtcNow);
            await publisher.PublishCompletionAsync(completion, default);
            foreach (var completionQueue in settings.CompletionQueueNames)
            {
                var delivered = await channel.BasicGetAsync(completionQueue, true);
                var decoded = delivered is null ? null :
                    JsonSerializer.Deserialize<foll_care.Shared.Infrastructure.Messaging.AccountDeletionCompleted>(delivered.Body.Span);
                check(decoded is not null && decoded.EventId == completion.EventId &&
                    decoded.OperationId == completion.OperationId && decoded.DeletedPatientIds.SequenceEqual(completion.DeletedPatientIds) &&
                    delivered!.BasicProperties.Persistent, "Care completion reaches durable " + completionQueue.Split('.').Last() + " queue");
            }
            foreach (var kind in new[] { "created", "accepted", "rejected" })
            {
                var message = Make(kind);
                await publisher.PublishAsync(message, default);
                var delivery = await channel.BasicGetAsync(settings.QueueName, autoAck: false);
                check(delivery is not null, "RabbitMQ delivers " + kind);
                var props = delivery!.BasicProperties;
                check(delivery.RoutingKey == message.EventType && props.Type == message.EventType &&
                    props.Persistent && props.ContentType == "application/json", "Routing, type and persistent JSON: " + kind);
                check(props.MessageId == message.EventId.ToString() && props.CorrelationId == message.OperationId.ToString(),
                    "Stable AMQP identity: " + kind);
                check(JsonSerializer.Deserialize<InvitationChangedIntegrationEvent>(delivery.Body.Span) == message, "Real broker JSON roundtrip: " + kind);
                await channel.BasicAckAsync(delivery.DeliveryTag, multiple: false);
                await publisher.PublishAsync(message, default);
                var repeated = await channel.BasicGetAsync(settings.QueueName, autoAck: true);
                check(repeated?.BasicProperties.MessageId == props.MessageId, "Retry retains EventId: " + kind);
            }
            bool returned = false;
            try { await transport.PublishAsync("unbound.test.v1", Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow, new byte[] { 1 }, default); }
            catch (PublishException ex) { returned = ex.IsReturn; }
            check(returned, "Mandatory unroutable publication fails instead of reporting success");
            var recovery = Make("created");
            await publisher.PublishAsync(recovery, default);
            check((await channel.BasicGetAsync(settings.QueueName, true))?.BasicProperties.MessageId == recovery.EventId.ToString(),
                "Transport reconnects after failed publication");
            using var canceled = new CancellationTokenSource(); canceled.Cancel();
            bool canceledCorrectly = false;
            try { await publisher.PublishAsync(Make("created"), canceled.Token); }
            catch (OperationCanceledException) { canceledCorrectly = true; }
            check(canceledCorrectly, "Publication respects caller cancellation");
            bool rejected = false;
            try { await publisher.PublishAsync(Make("created") with { Version = 99 }, default); }
            catch (InvalidOperationException) { rejected = true; }
            check(rejected, "Unsupported event version rejected before publishing");
            await Task.WhenAll(Enumerable.Range(0, 5).Select(_ => publisher.PublishAsync(Make("created"), default)));
            var ids = new HashSet<string>();
            for (int i = 0; i < 5; i++) ids.Add((await channel.BasicGetAsync(settings.QueueName, true))!.BasicProperties.MessageId!);
            check(ids.Count == 5, "Concurrent callers publish safely on serialized channel");
        }
        finally
        {
            foreach (var completionQueue in settings.CompletionQueueNames)
                await channel.QueueDeleteAsync(completionQueue, ifUnused: false, ifEmpty: false);
            await channel.QueueDeleteAsync(settings.QueueName, ifUnused: false, ifEmpty: false);
            await channel.ExchangeDeleteAsync(settings.ExchangeName, ifUnused: false);
        }
    }
}
