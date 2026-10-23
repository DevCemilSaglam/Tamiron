using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tamiron.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class HotQueryIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ServiceRecords_TenantId_DeliveredAt",
                table: "ServiceRecords",
                columns: new[] { "TenantId", "DeliveredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRecords_TenantId_PaymentCollectedAt",
                table: "ServiceRecords",
                columns: new[] { "TenantId", "PaymentCollectedAt" },
                filter: "[PaymentCollectedAt] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRecords_TenantId_ReminderDate",
                table: "ServiceRecords",
                columns: new[] { "TenantId", "ReminderDate" },
                filter: "[ReminderDate] IS NOT NULL AND [ReminderSent] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ServiceRecords_TenantId_DeliveredAt",
                table: "ServiceRecords");

            migrationBuilder.DropIndex(
                name: "IX_ServiceRecords_TenantId_PaymentCollectedAt",
                table: "ServiceRecords");

            migrationBuilder.DropIndex(
                name: "IX_ServiceRecords_TenantId_ReminderDate",
                table: "ServiceRecords");
        }
    }
}
