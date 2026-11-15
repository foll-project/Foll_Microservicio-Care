using foll_care.Care.Infrastructure.Messaging.Outbox;
using foll_care.Shared.Infrastructure.Persistence.EFC.Configuration;
using Microsoft.EntityFrameworkCore;
namespace foll_care.Shared.Infrastructure.Messaging;

public sealed class AccountDeletionRequestedHandler(AppDbContext db) : IBrokerMessageHandler
{
    public async Task HandleAsync(BrokerEnvelope envelope, CancellationToken ct)
    {
        if (envelope.EventType != AccountDeletionRequested.EventType) throw new InvalidDataException("Evento no soportado.");
        var message = envelope.Read<AccountDeletionRequested>();
        if (message.UserId <= 0) throw new InvalidDataException("UserId invalido.");
        if (await db.Set<IntegrationInboxMessage>().AnyAsync(x => x.OperationId == message.OperationId &&
            x.EventType == envelope.EventType, ct)) return;
        var patients = await db.Patients.Include(p => p.Caregivers)
            .Where(p => p.OfficialGuardianUserId == message.UserId ||
                p.CurrentGuardianUserId == message.UserId || p.Caregivers.Any(c => c.UserId == message.UserId)).ToListAsync(ct);
        var deletedIds = patients.Where(p => p.OfficialGuardianUserId == message.UserId).Select(p => p.PatientId).ToArray();
        foreach (var patient in patients)
        {
            if (patient.OfficialGuardianUserId == message.UserId) db.Patients.Remove(patient);
            else
            {
                patient.RemoveCaregiver(message.UserId);
                if (patient.CurrentGuardianUserId == message.UserId)
                    patient.RestoreGuardShift(patient.OfficialGuardianUserId);
            }
        }
        db.PatientInvitations.RemoveRange(await db.PatientInvitations.Where(x => x.InvitingUserId == message.UserId).ToListAsync(ct));
        db.RemoveRange(await db.Set<foll_care.Care.Domain.Model.Entities.PatientAnnotation>()
            .Where(x => x.AuthorUserId == message.UserId).ToListAsync(ct));
        db.IntegrationOutboxMessages.Add(IntegrationOutboxMessage.From(new AccountDeletionCompleted(
            Guid.NewGuid(), message.OperationId, message.UserId, deletedIds, DateTime.UtcNow)));
        // InboxProcessor commits cleanup, completion outbox and inbox in one transaction.
    }
}

