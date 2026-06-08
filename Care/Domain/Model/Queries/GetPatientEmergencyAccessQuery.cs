namespace foll_care.Care.Domain.Model.Queries;

public record GetPatientEmergencyAccessQuery(long ActorUserId, long PatientId);
