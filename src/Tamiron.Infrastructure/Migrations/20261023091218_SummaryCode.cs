using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tamiron.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SummaryCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SummaryCode",
                table: "ServiceRecords",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRecords_SummaryCode",
                table: "ServiceRecords",
                column: "SummaryCode",
                unique: true,
                filter: "[SummaryCode] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ServiceRecords_SummaryCode",
                table: "ServiceRecords");

            migrationBuilder.DropColumn(
                name: "SummaryCode",
                table: "ServiceRecords");
        }
    }
}
