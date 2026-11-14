using System.Text.Json.Serialization;

namespace foll_care.Care.Application.IntegrationEvents;

public record InvitationChangedIntegrationEvent(
    Guid EventId, Guid OperationId, long InvitationId, long PatientId,
    string PatientName, long TargetUserId, long RequesterUserId,
    short RelationshipTypeId, string RelationshipName, string Status,
    DateTime ExpiresAt, string Kind, DateTime OccurredAtUtc, int Version = 1)
{
    public const string CreatedEventType = "care.invitation-created.v1";
    public const string AcceptedEventType = "care.invitation-accepted.v1";
    public const string RejectedEventType = "care.invitation-rejected.v1";

    [JsonIgnore]
    public string EventType => Kind switch
    {
        "created" => CreatedEventType,
        "accepted" => AcceptedEventType,
        "rejected" => RejectedEventType,
        _ => throw new InvalidOperationException("Tipo de cambio de invitacion no soportado.")
    };
}