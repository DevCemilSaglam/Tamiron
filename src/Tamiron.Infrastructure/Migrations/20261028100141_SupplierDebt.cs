using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tamiron.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SupplierDebt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "OnCredit",
                table: "ServiceLines",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "SupplierId",
                table: "ServiceLines",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SupplierPayments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SupplierId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaidOn = table.Column<DateOnly>(type: "date", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Method = table.Column<int>(type: "int", nullable: false),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateTimeUpdated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeDeleted = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierPayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupplierPayments_Shops_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Shops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceLines_TenantId_SupplierId",
                table: "ServiceLines",
                columns: new[] { "TenantId", "SupplierId" },
                filter: "[OnCredit] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierPayments_TenantId",
                table: "SupplierPayments",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierPayments_TenantId_PaidOn",
                table: "SupplierPayments",
                columns: new[] { "TenantId", "PaidOn" });

            migrationBuilder.CreateIndex(
                name: "IX_SupplierPayments_TenantId_SupplierId",
                table: "SupplierPayments",
                columns: new[] { "TenantId", "SupplierId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SupplierPayments");

            migrationBuilder.DropIndex(
                name: "IX_ServiceLines_TenantId_SupplierId",
                table: "ServiceLines");

            migrationBuilder.DropColumn(
                name: "OnCredit",
                table: "ServiceLines");

            migrationBuilder.DropColumn(
                name: "SupplierId",
                table: "ServiceLines");
        }
    }
}
