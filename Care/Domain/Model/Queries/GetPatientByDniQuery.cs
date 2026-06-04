namespace foll_care.Care.Domain.Model.Queries;

public record GetPatientByDniQuery(long ActorUserId, string Dni);
