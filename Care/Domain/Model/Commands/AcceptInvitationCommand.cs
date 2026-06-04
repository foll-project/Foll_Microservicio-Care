namespace foll_care.Care.Domain.Model.Commands;

public record AcceptInvitationCommand(long ActorUserId, long InvitationId);
