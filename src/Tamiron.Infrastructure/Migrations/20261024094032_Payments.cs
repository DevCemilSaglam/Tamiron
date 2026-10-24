using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tamiron.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Payments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ServiceRecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Method = table.Column<int>(type: "int", nullable: false),
                    PaidOn = table.Column<DateOnly>(type: "date", nullable: false),
                    AtDelivery = table.Column<bool>(type: "bit", nullable: false),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateTimeUpdated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeDeleted = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payments_ServiceRecords_ServiceRecordId",
                        column: x => x.ServiceRecordId,
                        principalTable: "ServiceRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Payments_Shops_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Shops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Payments_ServiceRecordId",
                table: "Payments",
                column: "ServiceRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_TenantId",
                table: "Payments",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_TenantId_PaidOn",
                table: "Payments",
                columns: new[] { "TenantId", "PaidOn" });

            // F4 · K-078: var olan kayıtlara ödeme satırı. Teslimde ödenmiş → (toplam, yol, teslim günü,
            // teslimde); sonradan ödenmiş → (toplam, yol, tahsil günü, sonradan); veresiye → satır yok.
            // Toplam domain'deki ServiceRecord.Total'ın aynısı: silinmiş satır ve müşteri parçası
            // (Kind=Part, SuppliedBy=Customer) girmez. 0 ₺'lik işe satır yazılmaz (ödeme > 0).
            // Kasa göçten önce ve sonra aynı — MigrationUpgradeTests ölçer.
            migrationBuilder.Sql(@"
INSERT INTO Payments (Id, ServiceRecordId, Amount, Method, PaidOn, AtDelivery, DateTimeCreated, TenantId)
SELECT NEWID(), r.Id, t.Total, r.PaymentMethod, COALESCE(r.PaymentCollectedAt, r.DeliveredAt),
       CASE WHEN r.PaymentCollectedAt IS NULL THEN 1 ELSE 0 END, SYSUTCDATETIME(), r.TenantId
FROM ServiceRecords r
CROSS APPLY (SELECT SUM(l.Price) AS Total FROM ServiceLines l
             WHERE l.ServiceRecordId = r.Id AND l.DateTimeDeleted IS NULL AND NOT (l.Kind = 1 AND l.SuppliedBy = 2)) t
WHERE r.DeliveredAt IS NOT NULL AND r.PaymentMethod IS NOT NULL AND r.PaymentMethod <> 4
  AND r.DateTimeDeleted IS NULL AND t.Total > 0;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Payments");
        }
    }
}
