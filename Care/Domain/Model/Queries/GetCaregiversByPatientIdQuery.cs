namespace foll_care.Care.Domain.Model.Queries;

public record GetCaregiversByPatientIdQuery(long ActorUserId, long PatientId);