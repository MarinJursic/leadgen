using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeadGen.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialLeadGenMvp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AiCallLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Purpose = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    Provider = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    Model = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    InputTokens = table.Column<int>(type: "INTEGER", nullable: true),
                    OutputTokens = table.Column<int>(type: "INTEGER", nullable: true),
                    EstimatedCostUsd = table.Column<decimal>(type: "TEXT", precision: 10, scale: 6, nullable: false),
                    DurationMs = table.Column<int>(type: "INTEGER", nullable: false),
                    Success = table.Column<bool>(type: "INTEGER", nullable: false),
                    ErrorMessage = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiCallLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Campaigns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 160, nullable: false),
                    BusinessName = table.Column<string>(type: "TEXT", maxLength: 160, nullable: false),
                    WebsiteUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    BusinessDescription = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: false),
                    TargetGeography = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    TargetCustomers = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    Exclusions = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    IcpJson = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Campaigns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LeadSearchRuns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CampaignId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    RequestedLeadCount = table.Column<int>(type: "INTEGER", nullable: false),
                    SearchQueriesJson = table.Column<string>(type: "TEXT", nullable: true),
                    StartedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CompletedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ErrorMessage = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    EstimatedCostUsd = table.Column<decimal>(type: "TEXT", precision: 10, scale: 4, nullable: false),
                    LogsJson = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeadSearchRuns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeadSearchRuns_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Leads",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CampaignId = table.Column<Guid>(type: "TEXT", nullable: false),
                    LeadSearchRunId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CompanyName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Domain = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    WebsiteUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Industry = table.Column<string>(type: "TEXT", maxLength: 160, nullable: true),
                    Location = table.Column<string>(type: "TEXT", maxLength: 160, nullable: true),
                    FitScore = table.Column<int>(type: "INTEGER", nullable: false),
                    ConfidenceScore = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    MatchReasonsJson = table.Column<string>(type: "TEXT", nullable: false),
                    EvidenceJson = table.Column<string>(type: "TEXT", nullable: false),
                    DossierMarkdown = table.Column<string>(type: "TEXT", nullable: false),
                    SuggestedOutreachAngle = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Leads", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Leads_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Leads_LeadSearchRuns_LeadSearchRunId",
                        column: x => x.LeadSearchRunId,
                        principalTable: "LeadSearchRuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "LeadContacts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    LeadId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Type = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    Value = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    SourceUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    ConfidenceScore = table.Column<int>(type: "INTEGER", nullable: false),
                    IsVerified = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeadContacts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeadContacts_Leads_LeadId",
                        column: x => x.LeadId,
                        principalTable: "Leads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LeadNotes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    LeadId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Body = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeadNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeadNotes_Leads_LeadId",
                        column: x => x.LeadId,
                        principalTable: "Leads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AiCallLogs_CreatedAtUtc",
                table: "AiCallLogs",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_Campaigns_Name",
                table: "Campaigns",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_LeadContacts_LeadId",
                table: "LeadContacts",
                column: "LeadId");

            migrationBuilder.CreateIndex(
                name: "IX_LeadNotes_LeadId",
                table: "LeadNotes",
                column: "LeadId");

            migrationBuilder.CreateIndex(
                name: "IX_Leads_CampaignId_Domain",
                table: "Leads",
                columns: new[] { "CampaignId", "Domain" });

            migrationBuilder.CreateIndex(
                name: "IX_Leads_FitScore",
                table: "Leads",
                column: "FitScore");

            migrationBuilder.CreateIndex(
                name: "IX_Leads_LeadSearchRunId",
                table: "Leads",
                column: "LeadSearchRunId");

            migrationBuilder.CreateIndex(
                name: "IX_LeadSearchRuns_CampaignId_StartedAtUtc",
                table: "LeadSearchRuns",
                columns: new[] { "CampaignId", "StartedAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AiCallLogs");

            migrationBuilder.DropTable(
                name: "LeadContacts");

            migrationBuilder.DropTable(
                name: "LeadNotes");

            migrationBuilder.DropTable(
                name: "Leads");

            migrationBuilder.DropTable(
                name: "LeadSearchRuns");

            migrationBuilder.DropTable(
                name: "Campaigns");
        }
    }
}
