namespace foll_care.Care.Domain.Model.Commands;

public record RejectInvitationCommand(long ActorUserId, long InvitationId);
