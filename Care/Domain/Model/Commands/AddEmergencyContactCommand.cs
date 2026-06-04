namespace foll_care.Care.Domain.Model.Commands;

public record AddEmergencyContactCommand(
    long ActorUserId,
    long PatientId,
    string FullName,
    string PhoneNumber,
    string Relationship);
