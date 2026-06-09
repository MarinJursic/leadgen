using System.Text.Json;
using Leadgen.Model.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace leadgen.Data;

public sealed class LeadgenDbContext : IdentityDbContext<AppUser>
{
    public LeadgenDbContext(DbContextOptions<LeadgenDbContext> options)
        : base(options)
    {
    }

    public DbSet<BusinessDnaMission> BusinessDnaMissions => Set<BusinessDnaMission>();

    public DbSet<ClarificationQuestion> ClarificationQuestions => Set<ClarificationQuestion>();

    public DbSet<MissionRun> MissionRuns => Set<MissionRun>();

    public DbSet<MissionAgentAssignment> MissionAgentAssignments => Set<MissionAgentAssignment>();

    public DbSet<SwarmAgent> SwarmAgents => Set<SwarmAgent>();

    public DbSet<TargetCompany> TargetCompanies => Set<TargetCompany>();

    public DbSet<TargetContact> TargetContacts => Set<TargetContact>();

    public DbSet<ContactChannel> ContactChannels => Set<ContactChannel>();

    public DbSet<EvidencePoint> EvidencePoints => Set<EvidencePoint>();

    public DbSet<LeadDossier> LeadDossiers => Set<LeadDossier>();

    public DbSet<MissionAttachment> MissionAttachments => Set<MissionAttachment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var surfaceTagsComparer = new ValueComparer<List<string>>(
            (left, right) => left!.SequenceEqual(right!),
            value => value.Aggregate(0, (current, item) => HashCode.Combine(current, item.GetHashCode())),
            value => value.ToList());

        modelBuilder.Entity<BusinessDnaMission>(builder =>
        {
            builder.Property(mission => mission.SurfaceTags)
                .HasConversion(
                    value => JsonSerializer.Serialize(value, (JsonSerializerOptions?)null),
                    value => JsonSerializer.Deserialize<List<string>>(value, (JsonSerializerOptions?)null) ?? new List<string>())
                .Metadata.SetValueComparer(surfaceTagsComparer);

            builder.Property(mission => mission.ConfidenceScore).HasPrecision(5, 2);
        });

        modelBuilder.Entity<MissionRun>(builder =>
        {
            builder.HasIndex(run => run.RunCode).IsUnique();
            builder.Property(run => run.EstimatedCostUsd).HasPrecision(10, 2);

            builder.HasOne(run => run.Mission)
                .WithMany(mission => mission.Runs)
                .HasForeignKey(run => run.BusinessDnaMissionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ClarificationQuestion>(builder =>
        {
            builder.HasOne(question => question.Mission)
                .WithMany(mission => mission.ClarificationQuestions)
                .HasForeignKey(question => question.BusinessDnaMissionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<MissionAttachment>(builder =>
        {
            builder.HasOne(attachment => attachment.Mission)
                .WithMany(mission => mission.Attachments)
                .HasForeignKey(attachment => attachment.BusinessDnaMissionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SwarmAgent>(builder =>
        {
            builder.HasIndex(agent => agent.CodeName).IsUnique();
            builder.Property(agent => agent.Temperature).HasPrecision(4, 2);
        });

        modelBuilder.Entity<MissionAgentAssignment>(builder =>
        {
            builder.HasOne(assignment => assignment.MissionRun)
                .WithMany(run => run.AgentAssignments)
                .HasForeignKey(assignment => assignment.MissionRunId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(assignment => assignment.SwarmAgent)
                .WithMany(agent => agent.MissionAssignments)
                .HasForeignKey(assignment => assignment.SwarmAgentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TargetCompany>(builder =>
        {
            builder.Property(company => company.MatchScore).HasPrecision(5, 2);

            builder.HasOne(company => company.MissionRun)
                .WithMany(run => run.TargetCompanies)
                .HasForeignKey(company => company.MissionRunId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TargetContact>(builder =>
        {
            builder.HasOne(contact => contact.TargetCompany)
                .WithMany(company => company.Contacts)
                .HasForeignKey(contact => contact.TargetCompanyId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ContactChannel>(builder =>
        {
            builder.Property(channel => channel.ConfidenceScore).HasPrecision(5, 2);

            builder.HasOne(channel => channel.TargetContact)
                .WithMany(contact => contact.ContactChannels)
                .HasForeignKey(channel => channel.TargetContactId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<EvidencePoint>(builder =>
        {
            builder.Property(evidence => evidence.ConfidenceScore).HasPrecision(5, 2);

            builder.HasOne(evidence => evidence.TargetContact)
                .WithMany(contact => contact.EvidencePoints)
                .HasForeignKey(evidence => evidence.TargetContactId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<LeadDossier>(builder =>
        {
            builder.HasOne(dossier => dossier.MissionRun)
                .WithMany(run => run.LeadDossiers)
                .HasForeignKey(dossier => dossier.MissionRunId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(dossier => dossier.TargetCompany)
                .WithMany(company => company.LeadDossiers)
                .HasForeignKey(dossier => dossier.TargetCompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(dossier => dossier.TargetContact)
                .WithMany(contact => contact.LeadDossiers)
                .HasForeignKey(dossier => dossier.TargetContactId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
