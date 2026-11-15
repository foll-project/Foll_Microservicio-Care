using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using foll_care.Shared.Infrastructure.Messaging;
using foll_care.Shared.Infrastructure.Persistence.EFC.Configuration;
using foll_care.Care.Domain.Model.Entities;
static class Fixture
{
    static long ownId, otherId;
    public static IBrokerMessageHandler Handler(AppDbContext db) => new AccountDeletionRequestedHandler(db);
    public static async Task<BrokerEnvelope> SeedAsync(AppDbContext db)
    {
        var own = new Patient(Guid.NewGuid().ToString("N")[..8], "Paciente", "Principal", new DateOnly(1960, 1, 1), 101);
        var other = new Patient(Guid.NewGuid().ToString("N")[..8], "Paciente", "Ajeno", new DateOnly(1960, 1, 1), 202);
        other.AddCaregiver(101, 1);
        other.AssignGuardShift(101, 202);
        db.AddRange(own, other);
        await db.SaveChangesAsync();
        ownId = own.PatientId; otherId = other.PatientId;
        db.PatientInvitations.Add(new PatientInvitation(otherId, 101, 1, DateTime.UtcNow.AddDays(1)));
        other.AddAnnotation(101, "Anotacion del usuario.");
        var e = new AccountDeletionRequested(Guid.NewGuid(), Guid.NewGuid(), 101, DateTime.UtcNow);
        return new(e.EventId, e.OperationId, AccountDeletionRequested.EventType, JsonSerializer.Serialize(e));
    }
    public static async Task AssertAsync(AppDbContext db, Action<bool,string> check)
    {
        check(!await db.Patients.AnyAsync(p => p.PatientId == ownId), "official guardian's patients deleted");
        var other = await db.Patients.Include(p => p.Caregivers).SingleAsync(p => p.PatientId == otherId);
        check(other.OfficialGuardianUserId == 202 && other.CurrentGuardianUserId == 202 && other.Caregivers.All(c => c.UserId != 101),
            "other guardian's patient preserved; caregiver removed and shift restored");
        check(!await db.PatientInvitations.AnyAsync(x => x.InvitingUserId == 101), "requester's invitations removed");
        check(!await db.Set<PatientAnnotation>().AnyAsync(x => x.AuthorUserId == 101), "user annotations removed");
        var completion = await db.IntegrationOutboxMessages.SingleAsync();
        var body = JsonSerializer.Deserialize<AccountDeletionCompleted>(completion.Payload)!;
        check(completion.PublishedAtUtc is null && body.UserId == 101 && body.DeletedPatientIds.SequenceEqual(new[] { ownId }),
            "completion and deleted patient ids staged in durable outbox");
    }
}

