namespace foll_care.Care.Domain.Model.Commands;

public record AddPatientAnnotationCommand(long ActorUserId, long PatientId, string Content);