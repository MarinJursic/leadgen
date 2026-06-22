using LeadGen.Core.Domain;
using Microsoft.EntityFrameworkCore;

namespace LeadGen.Infrastructure.Data;

public sealed class LeadGenDbContext : DbContext
{
    public LeadGenDbContext(DbContextOptions<LeadGenDbContext> options)
        : base(options)
    {
    }

    public DbSet<Campaign> Campaigns => Set<Campaign>();

    public DbSet<LeadSearchRun> LeadSearchRuns => Set<LeadSearchRun>();

    public DbSet<Lead> Leads => Set<Lead>();

    public DbSet<LeadContact> LeadContacts => Set<LeadContact>();

    public DbSet<LeadNote> LeadNotes => Set<LeadNote>();

    public DbSet<AiCallLog> AiCallLogs => Set<AiCallLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Campaign>(builder =>
        {
            builder.HasKey(campaign => campaign.Id);
            builder.Property(campaign => campaign.Name).HasMaxLength(160).IsRequired();
            builder.Property(campaign => campaign.BusinessName).HasMaxLength(160).IsRequired();
            builder.Property(campaign => campaign.WebsiteUrl).HasMaxLength(500);
            builder.Property(campaign => campaign.BusinessDescription).HasMaxLength(4000).IsRequired();
            builder.Property(campaign => campaign.TargetGeography).HasMaxLength(500);
            builder.Property(campaign => campaign.TargetCustomers).HasMaxLength(1000);
            builder.Property(campaign => campaign.Exclusions).HasMaxLength(1000);
            builder.HasIndex(campaign => campaign.Name);
        });

        modelBuilder.Entity<LeadSearchRun>(builder =>
        {
            builder.HasKey(run => run.Id);
            builder.Property(run => run.Status).HasConversion<string>().HasMaxLength(40);
            builder.Property(run => run.ErrorMessage).HasMaxLength(1000);
            builder.Property(run => run.EstimatedCostUsd).HasPrecision(10, 4);
            builder.HasOne(run => run.Campaign)
                .WithMany(campaign => campaign.Runs)
                .HasForeignKey(run => run.CampaignId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasIndex(run => new { run.CampaignId, run.StartedAtUtc });
        });

        modelBuilder.Entity<Lead>(builder =>
        {
            builder.HasKey(lead => lead.Id);
            builder.Property(lead => lead.CompanyName).HasMaxLength(200).IsRequired();
            builder.Property(lead => lead.Domain).HasMaxLength(200);
            builder.Property(lead => lead.DedupeKey).HasMaxLength(260).IsRequired();
            builder.Property(lead => lead.WebsiteUrl).HasMaxLength(500);
            builder.Property(lead => lead.Industry).HasMaxLength(160);
            builder.Property(lead => lead.Location).HasMaxLength(160);
            builder.Property(lead => lead.Status).HasConversion<string>().HasMaxLength(40);
            builder.Property(lead => lead.DossierMarkdown).IsRequired();
            builder.Property(lead => lead.SuggestedOutreachAngle).HasMaxLength(1000);
            builder.HasOne(lead => lead.Campaign)
                .WithMany(campaign => campaign.Leads)
                .HasForeignKey(lead => lead.CampaignId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(lead => lead.LeadSearchRun)
                .WithMany(run => run.Leads)
                .HasForeignKey(lead => lead.LeadSearchRunId)
                .OnDelete(DeleteBehavior.SetNull);
            builder.HasIndex(lead => new { lead.CampaignId, lead.Domain });
            builder.HasIndex(lead => new { lead.CampaignId, lead.DedupeKey });
            builder.HasIndex(lead => lead.FitScore);
        });

        modelBuilder.Entity<LeadContact>(builder =>
        {
            builder.HasKey(contact => contact.Id);
            builder.Property(contact => contact.Type).HasConversion<string>().HasMaxLength(40);
            builder.Property(contact => contact.Value).HasMaxLength(500).IsRequired();
            builder.Property(contact => contact.SourceUrl).HasMaxLength(500);
            builder.HasOne(contact => contact.Lead)
                .WithMany(lead => lead.Contacts)
                .HasForeignKey(contact => contact.LeadId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<LeadNote>(builder =>
        {
            builder.HasKey(note => note.Id);
            builder.Property(note => note.Body).HasMaxLength(4000).IsRequired();
            builder.HasOne(note => note.Lead)
                .WithMany(lead => lead.Notes)
                .HasForeignKey(note => note.LeadId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AiCallLog>(builder =>
        {
            builder.HasKey(log => log.Id);
            builder.Property(log => log.Purpose).HasMaxLength(80).IsRequired();
            builder.Property(log => log.Provider).HasMaxLength(80).IsRequired();
            builder.Property(log => log.Model).HasMaxLength(120).IsRequired();
            builder.Property(log => log.EstimatedCostUsd).HasPrecision(10, 6);
            builder.Property(log => log.ErrorMessage).HasMaxLength(1000);
            builder.HasIndex(log => log.CreatedAtUtc);
        });
    }
}
