using foll_care.Care.Application.IntegrationEvents;

namespace foll_care.Care.Application.OutboundServices;

public interface IIntegrationEventPublisher
{
    // Complete only after durable transport acceptance. Failures must throw.
    // Retries must preserve EventId and OperationId.
    Task PublishAsync(InvitationChangedIntegrationEvent integrationEvent, CancellationToken cancellationToken);
}