using foll_care.Care.Application.IntegrationEvents;

namespace foll_care.Care.Application.OutboundServices;

public interface ICareEventService
{
    // Stages the event in the same unit of work as the invitation change.
    Task InvitationChangedAsync(InvitationChangedIntegrationEvent integrationEvent);
}