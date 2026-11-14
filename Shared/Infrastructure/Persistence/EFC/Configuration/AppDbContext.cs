
using foll_care.Care.Domain.Model.Entities;
using Microsoft.EntityFrameworkCore;

namespace foll_care.Shared.Infrastructure.Persistence.EFC.Configuration;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }


    public DbSet<foll_care.Care.Infrastructure.Messaging.Outbox.IntegrationOutboxMessage> IntegrationOutboxMessages { get; set; }

    public DbSet<Patient> Patients { get; set; }
    public DbSet<PatientInvitation> PatientInvitations { get; set; }
    public DbSet<EmergencyContact> EmergencyContacts { get; set; }
    public DbSet<RelationshipType> RelationshipTypes { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}

