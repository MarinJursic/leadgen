using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace leadgen.Migrations
{
    /// <inheritdoc />
    public partial class InitialLeadgenSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BusinessDnaMissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    MissionName = table.Column<string>(type: "TEXT", maxLength: 160, nullable: false),
                    ProductName = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    Mechanic = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    PrimarySurface = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    SurfaceTags = table.Column<string>(type: "TEXT", nullable: false),
                    Persona = table.Column<string>(type: "TEXT", maxLength: 240, nullable: false),
                    Villain = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Delta = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    ConfidenceScore = table.Column<decimal>(type: "TEXT", precision: 5, scale: 2, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessDnaMissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SwarmAgents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CodeName = table.Column<string>(type: "TEXT", maxLength: 60, nullable: false),
                    Role = table.Column<int>(type: "INTEGER", nullable: false),
                    Provider = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    Temperature = table.Column<decimal>(type: "TEXT", precision: 4, scale: 2, nullable: false),
                    MaxConcurrentTasks = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    LastHeartbeatUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CurrentFocus = table.Column<string>(type: "TEXT", maxLength: 320, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SwarmAgents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ClarificationQuestions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    BusinessDnaMissionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SlotName = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    Prompt = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Reason = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    IsAnswered = table.Column<bool>(type: "INTEGER", nullable: false),
                    Answer = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    AnsweredAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClarificationQuestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClarificationQuestions_BusinessDnaMissions_BusinessDnaMissionId",
                        column: x => x.BusinessDnaMissionId,
                        principalTable: "BusinessDnaMissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MissionRuns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    RunCode = table.Column<string>(type: "TEXT", maxLength: 60, nullable: false),
                    BusinessDnaMissionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    StartedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CompletedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    SearchRegion = table.Column<string>(type: "TEXT", maxLength: 160, nullable: false),
                    TokenBudget = table.Column<int>(type: "INTEGER", nullable: false),
                    EstimatedCostUsd = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MissionRuns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MissionRuns_BusinessDnaMissions_BusinessDnaMissionId",
                        column: x => x.BusinessDnaMissionId,
                        principalTable: "BusinessDnaMissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MissionAgentAssignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    MissionRunId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SwarmAgentId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AssignedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Responsibility = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    TokenBudget = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MissionAgentAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MissionAgentAssignments_MissionRuns_MissionRunId",
                        column: x => x.MissionRunId,
                        principalTable: "MissionRuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MissionAgentAssignments_SwarmAgents_SwarmAgentId",
                        column: x => x.SwarmAgentId,
                        principalTable: "SwarmAgents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TargetCompanies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    MissionRunId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 180, nullable: false),
                    Domain = table.Column<string>(type: "TEXT", maxLength: 180, nullable: false),
                    Industry = table.Column<string>(type: "TEXT", maxLength: 160, nullable: false),
                    HeadquartersCity = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    HeadquartersCountry = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    OrganizationStageLabel = table.Column<string>(type: "TEXT", maxLength: 120, nullable: true),
                    LastSignalAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    EmployeeCount = table.Column<int>(type: "INTEGER", nullable: false),
                    IsHeadquartersVerified = table.Column<bool>(type: "INTEGER", nullable: false),
                    MatchScore = table.Column<decimal>(type: "TEXT", precision: 5, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TargetCompanies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TargetCompanies_MissionRuns_MissionRunId",
                        column: x => x.MissionRunId,
                        principalTable: "MissionRuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TargetContacts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TargetCompanyId = table.Column<Guid>(type: "TEXT", nullable: false),
                    FullName = table.Column<string>(type: "TEXT", maxLength: 180, nullable: false),
                    JobTitle = table.Column<string>(type: "TEXT", maxLength: 180, nullable: false),
                    Department = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    Seniority = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    IsDecisionMaker = table.Column<bool>(type: "INTEGER", nullable: false),
                    LinkedInUrl = table.Column<string>(type: "TEXT", maxLength: 320, nullable: true),
                    XHandle = table.Column<string>(type: "TEXT", maxLength: 120, nullable: true),
                    GitHubUsername = table.Column<string>(type: "TEXT", maxLength: 120, nullable: true),
                    OpportunitySummary = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    LastObservedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TargetContacts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TargetContacts_TargetCompanies_TargetCompanyId",
                        column: x => x.TargetCompanyId,
                        principalTable: "TargetCompanies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContactChannels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TargetContactId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Value = table.Column<string>(type: "TEXT", maxLength: 320, nullable: false),
                    IsVerified = table.Column<bool>(type: "INTEGER", nullable: false),
                    VerifiedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Source = table.Column<string>(type: "TEXT", maxLength: 160, nullable: false),
                    ConfidenceScore = table.Column<decimal>(type: "TEXT", precision: 5, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactChannels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContactChannels_TargetContacts_TargetContactId",
                        column: x => x.TargetContactId,
                        principalTable: "TargetContacts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EvidencePoints",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TargetContactId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Kind = table.Column<int>(type: "INTEGER", nullable: false),
                    Label = table.Column<string>(type: "TEXT", maxLength: 160, nullable: false),
                    SourcePlatform = table.Column<string>(type: "TEXT", maxLength: 160, nullable: false),
                    SourceUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Summary = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    RawSnippet = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    CapturedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ConfidenceScore = table.Column<decimal>(type: "TEXT", precision: 5, scale: 2, nullable: false),
                    IsQualificationSignal = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EvidencePoints", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EvidencePoints_TargetContacts_TargetContactId",
                        column: x => x.TargetContactId,
                        principalTable: "TargetContacts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LeadDossiers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    MissionRunId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TargetCompanyId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TargetContactId = table.Column<Guid>(type: "TEXT", nullable: false),
                    LeadgenScore = table.Column<int>(type: "INTEGER", nullable: false),
                    SuggestedApproach = table.Column<string>(type: "TEXT", maxLength: 600, nullable: false),
                    AdvantagePoint = table.Column<string>(type: "TEXT", maxLength: 600, nullable: false),
                    IsReadyForOutreach = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastUpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SupportingEvidenceCount = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeadDossiers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeadDossiers_MissionRuns_MissionRunId",
                        column: x => x.MissionRunId,
                        principalTable: "MissionRuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LeadDossiers_TargetCompanies_TargetCompanyId",
                        column: x => x.TargetCompanyId,
                        principalTable: "TargetCompanies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LeadDossiers_TargetContacts_TargetContactId",
                        column: x => x.TargetContactId,
                        principalTable: "TargetContacts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClarificationQuestions_BusinessDnaMissionId",
                table: "ClarificationQuestions",
                column: "BusinessDnaMissionId");

            migrationBuilder.CreateIndex(
                name: "IX_ContactChannels_TargetContactId",
                table: "ContactChannels",
                column: "TargetContactId");

            migrationBuilder.CreateIndex(
                name: "IX_EvidencePoints_TargetContactId",
                table: "EvidencePoints",
                column: "TargetContactId");

            migrationBuilder.CreateIndex(
                name: "IX_LeadDossiers_MissionRunId",
                table: "LeadDossiers",
                column: "MissionRunId");

            migrationBuilder.CreateIndex(
                name: "IX_LeadDossiers_TargetCompanyId",
                table: "LeadDossiers",
                column: "TargetCompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_LeadDossiers_TargetContactId",
                table: "LeadDossiers",
                column: "TargetContactId");

            migrationBuilder.CreateIndex(
                name: "IX_MissionAgentAssignments_MissionRunId",
                table: "MissionAgentAssignments",
                column: "MissionRunId");

            migrationBuilder.CreateIndex(
                name: "IX_MissionAgentAssignments_SwarmAgentId",
                table: "MissionAgentAssignments",
                column: "SwarmAgentId");

            migrationBuilder.CreateIndex(
                name: "IX_MissionRuns_BusinessDnaMissionId",
                table: "MissionRuns",
                column: "BusinessDnaMissionId");

            migrationBuilder.CreateIndex(
                name: "IX_MissionRuns_RunCode",
                table: "MissionRuns",
                column: "RunCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SwarmAgents_CodeName",
                table: "SwarmAgents",
                column: "CodeName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TargetCompanies_MissionRunId",
                table: "TargetCompanies",
                column: "MissionRunId");

            migrationBuilder.CreateIndex(
                name: "IX_TargetContacts_TargetCompanyId",
                table: "TargetContacts",
                column: "TargetCompanyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClarificationQuestions");

            migrationBuilder.DropTable(
                name: "ContactChannels");

            migrationBuilder.DropTable(
                name: "EvidencePoints");

            migrationBuilder.DropTable(
                name: "LeadDossiers");

            migrationBuilder.DropTable(
                name: "MissionAgentAssignments");

            migrationBuilder.DropTable(
                name: "TargetContacts");

            migrationBuilder.DropTable(
                name: "SwarmAgents");

            migrationBuilder.DropTable(
                name: "TargetCompanies");

            migrationBuilder.DropTable(
                name: "MissionRuns");

            migrationBuilder.DropTable(
                name: "BusinessDnaMissions");
        }
    }
}
