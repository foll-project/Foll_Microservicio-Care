using System.Text.Json;
using foll_care.Care.Application.IntegrationEvents;
using foll_care.Care.Application.OutboundServices;

namespace foll_care.Care.Infrastructure.Messaging.RabbitMq;

public sealed class RabbitMqIntegrationEventPublisher(RabbitMqConnection connection) : IIntegrationEventPublisher
{
    public Task PublishCompletionAsync(foll_care.Shared.Infrastructure.Messaging.AccountDeletionCompleted e, CancellationToken ct)
    {
        if (e.Version != 1 || e.EventId == Guid.Empty || e.OperationId == Guid.Empty || e.UserId <= 0)
            throw new InvalidOperationException("Confirmacion de Care invalida.");
        return connection.PublishAsync(foll_care.Shared.Infrastructure.Messaging.AccountDeletionCompleted.EventType,
            e.EventId, e.OperationId, e.OccurredAtUtc, JsonSerializer.SerializeToUtf8Bytes(e), ct);
    }

    public Task PublishAsync(InvitationChangedIntegrationEvent integrationEvent, CancellationToken cancellationToken)
    {
        if (integrationEvent.Version != 1 || integrationEvent.EventId == Guid.Empty ||
            integrationEvent.OperationId == Guid.Empty)
            throw new InvalidOperationException("Evento de integracion invalido o version no soportada.");

        return connection.PublishAsync(integrationEvent.EventType, integrationEvent.EventId,
            integrationEvent.OperationId, integrationEvent.OccurredAtUtc,
            JsonSerializer.SerializeToUtf8Bytes(integrationEvent), cancellationToken);
    }
}
