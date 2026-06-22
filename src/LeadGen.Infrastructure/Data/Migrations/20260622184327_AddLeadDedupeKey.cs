using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeadGen.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLeadDedupeKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DedupeKey",
                table: "Leads",
                type: "TEXT",
                maxLength: 260,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("""
                UPDATE Leads
                SET DedupeKey =
                    CASE
                        WHEN Domain IS NOT NULL AND trim(Domain) <> '' THEN
                            'domain:' || lower(replace(replace(replace(trim(Domain), 'https://', ''), 'http://', ''), 'www.', ''))
                        WHEN Location IS NOT NULL AND trim(Location) <> '' THEN
                            'name:' || lower(trim(CompanyName)) || ':location:' || lower(trim(Location))
                        ELSE
                            'name:' || lower(trim(CompanyName))
                    END
                WHERE DedupeKey = '';
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Leads_CampaignId_DedupeKey",
                table: "Leads",
                columns: new[] { "CampaignId", "DedupeKey" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Leads_CampaignId_DedupeKey",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "DedupeKey",
                table: "Leads");
        }
    }
}
