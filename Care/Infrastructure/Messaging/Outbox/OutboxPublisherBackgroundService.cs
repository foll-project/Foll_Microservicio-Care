using System.Text.Json;
using foll_care.Care.Application.IntegrationEvents;
using foll_care.Care.Application.OutboundServices;
using foll_care.Care.Infrastructure.Messaging.Configuration;
using foll_care.Shared.Infrastructure.Persistence.EFC.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace foll_care.Care.Infrastructure.Messaging.Outbox;

public class OutboxPublisherBackgroundService(
    IServiceScopeFactory scopeFactory,
    IOptions<IntegrationMessagingOptions> options,
    ILogger<OutboxPublisherBackgroundService> logger) : BackgroundService
{
    public override Task StartAsync(CancellationToken cancellationToken)
    {
        // Fail fast rather than silently claiming publication without a transport.
        if (options.Value.Enabled)
        {
            using var scope = scopeFactory.CreateScope();
            if (scope.ServiceProvider.GetService<IIntegrationEventPublisher>() is null)
                throw new InvalidOperationException("IntegrationMessaging esta habilitado sin un adaptador IIntegrationEventPublisher.");
        }
        return base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!options.Value.Enabled)
        {
            logger.LogInformation("Publicacion deshabilitada. Los eventos permanecen pendientes en el outbox.");
            return;
        }
        while (!stoppingToken.IsCancellationRequested)
        {
            try { await PublishBatchAsync(stoppingToken); }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            catch (Exception ex) { logger.LogError(ex, "Error procesando el outbox de Care."); }
            try { await Task.Delay(TimeSpan.FromSeconds(options.Value.PollIntervalSeconds), stoppingToken); }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
        }
    }

    private async Task PublishBatchAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var publisher = scope.ServiceProvider.GetRequiredService<IIntegrationEventPublisher>();
        // Serializes dispatchers across replicas. At-least-once delivery still requires consumer deduplication.
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        var messages = await context.IntegrationOutboxMessages.FromSqlInterpolated($"SELECT * FROM care.integration_outbox_messages WHERE published_at_utc IS NULL AND next_attempt_at_utc <= {DateTime.UtcNow} ORDER BY occurred_at_utc LIMIT {options.Value.BatchSize} FOR UPDATE SKIP LOCKED")
            .ToListAsync(cancellationToken);
        foreach (var message in messages)
        {
            try
            {
                if (message.Version != 1)
                    throw new InvalidOperationException("Contrato de evento no soportado.");
                if (message.EventType == foll_care.Shared.Infrastructure.Messaging.AccountDeletionCompleted.EventType)
                {
                    var completion = JsonSerializer.Deserialize<foll_care.Shared.Infrastructure.Messaging.AccountDeletionCompleted>(message.Payload)
                        ?? throw new InvalidOperationException("Confirmacion vacia.");
                    if (completion.EventId != message.EventId || completion.OperationId != message.OperationId || completion.Version != message.Version)
                        throw new InvalidOperationException("Confirmacion no coincide con el outbox.");
                    await scope.ServiceProvider.GetRequiredService<foll_care.Care.Infrastructure.Messaging.RabbitMq.RabbitMqIntegrationEventPublisher>()
                        .PublishCompletionAsync(completion, cancellationToken);
                    message.MarkAsPublished(DateTime.UtcNow);
                    continue;
                }
                var integrationEvent = JsonSerializer.Deserialize<InvitationChangedIntegrationEvent>(message.Payload)
                    ?? throw new InvalidOperationException("Evento vacio en outbox.");
                if (integrationEvent.EventType != message.EventType || integrationEvent.EventId != message.EventId ||
                    integrationEvent.OperationId != message.OperationId || integrationEvent.Version != message.Version)
                    throw new InvalidOperationException("El contrato no coincide con los metadatos del outbox.");
                await publisher.PublishAsync(integrationEvent, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
            catch (Exception ex)
            {
                message.RecordFailure(DateTime.UtcNow, ex.Message);
                logger.LogWarning(ex, "Fallo publicacion del evento {EventId}; se reintentara.", message.EventId);
                continue;
            }
            var now = DateTime.UtcNow;
            message.MarkAsPublished(now);
        }
        // A failed commit after transport acceptance causes redelivery with the same EventId.
        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }
}
