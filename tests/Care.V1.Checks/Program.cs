using System.Text.Json;
using foll_care.Care.Application.IntegrationEvents;
using foll_care.Care.Application.Internal.CommandServices;
using foll_care.Care.Application.OutboundServices;
using foll_care.Care.Domain.Model.Commands;
using foll_care.Care.Domain.Model.Entities;
using foll_care.Care.Domain.Model.Enums;
using foll_care.Care.Infrastructure.Messaging.Configuration;
using foll_care.Care.Infrastructure.Messaging.Outbox;
using foll_care.Care.Infrastructure.Persistence.EFC.Repositories;
using foll_care.Shared.Infrastructure.Persistence.EFC.Configuration;
using foll_care.Shared.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

// This value is exclusively for the isolated, in-memory test database.
Environment.SetEnvironmentVariable("Encryption__Key", "care-v1-checks-only-key-1234567890");
int count = 0;
void Check(bool condition, string name) {
    if (!condition) throw new Exception("FAIL: " + name);
    count++; Console.WriteLine("PASS: " + name);
}
async Task Throws<T>(Func<Task> action, string name) where T : Exception {
    try { await action(); } catch (T) { Check(true, name); return; }
    throw new Exception("Expected " + typeof(T).Name + ": " + name);
}
using var connection = new SqliteConnection("Data Source=:memory:");
connection.Open();
var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connection).UseSnakeCaseNamingConvention().Options;
await using var db = new AppDbContext(options);
await db.Database.EnsureCreatedAsync();
var patient = new Patient("12345678", "Ana", "Perez", new DateOnly(1950,1,1), 10, BloodType.Unknown, null, null);
patient.AddCaregiver(10, 1);
await db.Patients.AddAsync(patient);
await db.SaveChangesAsync();
PatientCommandService Service(ICareEventService? events = null) => new(
    new PatientRepository(db), new PatientInvitationRepository(db), new RelationshipTypeRepository(db),
    new UnitOfWork(db), events ?? new OutboxCareEventService(db));
var service = Service();
var expires = DateTime.UtcNow.AddDays(2);
await Throws<InvalidOperationException>(() => service.Handle(new CreateInvitationCommand(10, patient.Dni, 1, expires)), "already linked user cannot request access");
await Throws<InvalidOperationException>(() => service.Handle(new CreateInvitationCommand(20, patient.Dni, 99, expires)), "unknown relationship rejected");
Check(await db.IntegrationOutboxMessages.CountAsync() == 0, "invalid requests produce no events");
var invitationId = await service.Handle(new CreateInvitationCommand(20, patient.Dni, 1, expires));
db.ChangeTracker.Clear();
var created = await db.IntegrationOutboxMessages.SingleAsync();
var payload = JsonSerializer.Deserialize<InvitationChangedIntegrationEvent>(created.Payload)!;
Check(invitationId > 0 && payload.InvitationId == invitationId, "creation event contains persisted generated ID");
Check(payload.TargetUserId == 10 && payload.RequesterUserId == 20 && payload.PatientId == patient.PatientId, "creation is addressed to principal");
Check(created.EventType == InvitationChangedIntegrationEvent.CreatedEventType && payload.Status == "Pending", "created contract and state");
Check(payload.RelationshipName == "Hijo" && payload.PatientName == "Ana Perez", "Care-owned display data preserved");
Check(payload.EventId != Guid.Empty && payload.OperationId != Guid.Empty && payload.Version == 1, "version and correlation identifiers");
Check(created.PublishedAtUtc is null && created.AttemptCount == 0, "event remains pending without transport");
Check(!created.Payload.Contains("MedicalConditions") && !created.Payload.Contains("Authorization") && !created.Payload.Contains("Password"), "payload excludes clinical data and credentials");
var before = await db.IntegrationOutboxMessages.CountAsync();
await Throws<InvalidOperationException>(() => service.Handle(new AcceptInvitationCommand(30, invitationId)), "unrelated user cannot accept");
Check(await db.IntegrationOutboxMessages.CountAsync() == before, "unauthorized acceptance creates no event");
await service.Handle(new AcceptInvitationCommand(10, invitationId));
db.ChangeTracker.Clear();
var accepted = await db.IntegrationOutboxMessages.SingleAsync(e => e.EventType == InvitationChangedIntegrationEvent.AcceptedEventType);
var acceptedPayload = JsonSerializer.Deserialize<InvitationChangedIntegrationEvent>(accepted.Payload)!;
Check(acceptedPayload.TargetUserId == 20 && acceptedPayload.Kind == "accepted" && acceptedPayload.Status == "Accepted", "accepted event addressed to requester");
Check((await new PatientRepository(db).FindByIdAsync(patient.PatientId))!.Caregivers.Any(c => c.UserId == 20), "acceptance saves caregiver and event");
await Throws<InvalidOperationException>(() => service.Handle(new AcceptInvitationCommand(10, invitationId)), "repeat acceptance remains invalid");
Check(await db.IntegrationOutboxMessages.CountAsync() == 2, "repeat acceptance creates no duplicate event");
var rejectId = await service.Handle(new CreateInvitationCommand(30, patient.Dni, 2, expires));
await service.Handle(new RejectInvitationCommand(10, rejectId));
db.ChangeTracker.Clear();
var rejected = await db.IntegrationOutboxMessages.SingleAsync(e => e.EventType == InvitationChangedIntegrationEvent.RejectedEventType);
var rejectedPayload = JsonSerializer.Deserialize<InvitationChangedIntegrationEvent>(rejected.Payload)!;
Check(rejectedPayload.TargetUserId == 30 && rejectedPayload.Status == "Rejected", "rejection event addressed to requester");
Check(!(await new PatientRepository(db).FindByIdAsync(patient.PatientId))!.Caregivers.Any(c => c.UserId == 30), "rejection does not link requester");
await Throws<InvalidOperationException>(() => service.Handle(new RejectInvitationCommand(10, rejectId)), "repeat rejection remains invalid");
var rows = await db.PatientInvitations.CountAsync();
var messages = await db.IntegrationOutboxMessages.CountAsync();
await Throws<InvalidOperationException>(() => Service(new FailingEvents()).Handle(new CreateInvitationCommand(40, patient.Dni, 1, expires)), "outbox failure aborts creation");
db.ChangeTracker.Clear();
Check(await db.PatientInvitations.CountAsync() == rows && await db.IntegrationOutboxMessages.CountAsync() == messages, "real transaction rolls back the first save when event staging fails");
var failureId = await service.Handle(new CreateInvitationCommand(40, patient.Dni, 1, expires));
await Throws<InvalidOperationException>(() => Service(new FailingEvents()).Handle(new AcceptInvitationCommand(10, failureId)), "event failure aborts acceptance");
db.ChangeTracker.Clear();
Check((await db.PatientInvitations.FindAsync(failureId))!.Status == InvitationStatus.Pending && !(await new PatientRepository(db).FindByIdAsync(patient.PatientId))!.Caregivers.Any(c => c.UserId == 40), "failed acceptance persists neither state nor caregiver");
await Throws<InvalidOperationException>(() => Service(new FailingEvents()).Handle(new RejectInvitationCommand(10, failureId)), "event failure aborts rejection");
db.ChangeTracker.Clear();
Check((await db.PatientInvitations.FindAsync(failureId))!.Status == InvitationStatus.Pending, "failed rejection preserves pending status");
var retry = IntegrationOutboxMessage.From(payload);
var eventId = retry.EventId; var operationId = retry.OperationId;
retry.RecordFailure(DateTime.UtcNow, new string('x', 3000));
Check(retry.PublishedAtUtc is null && retry.AttemptCount == 1 && retry.LastError!.Length == 2000 && retry.NextAttemptAtUtc > DateTime.UtcNow, "failure keeps pending state and schedules backoff");
for(int i=0;i<12;i++) retry.RecordFailure(DateTime.UtcNow, "failed");
Check(retry.NextAttemptAtUtc <= DateTime.UtcNow.AddSeconds(300), "retry backoff capped at 300 seconds");
retry.MarkAsPublished(DateTime.UtcNow);
Check(retry.PublishedAtUtc is not null && retry.LastError is null && retry.EventId == eventId && retry.OperationId == operationId, "confirmed publication preserves identity");
await using var postgres = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseNpgsql("Host=localhost;Database=unused").UseSnakeCaseNamingConvention().Options);
var model = postgres.Model.FindEntityType(typeof(IntegrationOutboxMessage))!;
Check(model.GetSchema() == "care" && model.FindPrimaryKey()!.Properties.Single().Name == "EventId", "outbox schema and deduplication key");
Check(!model.GetIndexes().Single(i => i.Properties.Single().Name == "OperationId").IsUnique, "multiple events allowed per operation");
var sql = postgres.GetService<IMigrator>().GenerateScript();
Check(sql.Contains("care.integration_outbox_messages") && sql.Contains("jsonb"), "PostgreSQL migration generates outbox table");
using var provider = new ServiceCollection().BuildServiceProvider();
using var disabled = new OutboxPublisherBackgroundService(provider.GetRequiredService<IServiceScopeFactory>(), Options.Create(new IntegrationMessagingOptions()), NullLogger<OutboxPublisherBackgroundService>.Instance);
await disabled.StartAsync(default); await disabled.StopAsync(default);
Check(true, "disabled worker needs no publisher or database");
using var enabled = new OutboxPublisherBackgroundService(provider.GetRequiredService<IServiceScopeFactory>(), Options.Create(new IntegrationMessagingOptions { Enabled=true }), NullLogger<OutboxPublisherBackgroundService>.Instance);
await Throws<InvalidOperationException>(() => enabled.StartAsync(default), "enabled worker without transport fails explicitly");
Console.WriteLine($"{count} checks passed. SQLite transactions tested; no live PostgreSQL or broker used.");

sealed class FailingEvents : ICareEventService {
    public Task InvitationChangedAsync(InvitationChangedIntegrationEvent message) => throw new InvalidOperationException("Simulated outbox failure");
}