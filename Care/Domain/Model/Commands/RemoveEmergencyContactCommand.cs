namespace foll_care.Care.Domain.Model.Commands;

public record RemoveEmergencyContactCommand(long ActorUserId, long PatientId, long EmergencyContactId);
