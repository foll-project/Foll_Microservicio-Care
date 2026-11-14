using foll_care.Care.Application.IntegrationEvents;
using foll_care.Care.Application.OutboundServices;
using foll_care.Shared.Infrastructure.Persistence.EFC.Configuration;

namespace foll_care.Care.Infrastructure.Messaging.Outbox;

public class OutboxCareEventService(AppDbContext context) : ICareEventService
{
    public async Task InvitationChangedAsync(InvitationChangedIntegrationEvent integrationEvent) =>
        await context.IntegrationOutboxMessages.AddAsync(IntegrationOutboxMessage.From(integrationEvent));
}