namespace foll_care.Care.Domain.Model.Commands;

public record DeletePatientCommand(long UserId, long PatientId);
