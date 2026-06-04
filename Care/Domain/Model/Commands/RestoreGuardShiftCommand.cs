namespace foll_care.Care.Domain.Model.Commands;

public record RestoreGuardShiftCommand(long ActorUserId, long PatientId);
