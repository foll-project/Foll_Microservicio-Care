namespace foll_care.Care.Domain.Model.Queries;

public record GetInvitationsByPatientDniQuery(long ActorUserId, string PatientDni);
