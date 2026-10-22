using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tamiron.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Shops",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    DefaultMaintenanceMonths = table.Column<int>(type: "int", nullable: false),
                    DefaultMaintenanceKm = table.Column<int>(type: "int", nullable: false),
                    AskPartCost = table.Column<bool>(type: "bit", nullable: false),
                    PrivacyNoticeShownAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateTimeUpdated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeDeleted = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shops", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Vehicles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlateNo = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    Brand = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Model = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Year = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Engine = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Vin = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    Color = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    FuelType = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    EngineNo = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    Displacement = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    PowerKw = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Seats = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    FirstRegistrationDate = table.Column<DateOnly>(type: "date", nullable: true),
                    FuelTankLiters = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    OwnerName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    OwnerPhone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    ContactName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ContactPhone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    TaxOrNationalId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TaxOffice = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Note = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    MarketingConsentAt = table.Column<DateOnly>(type: "date", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateTimeUpdated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeDeleted = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vehicles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Vehicles_Shops_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Shops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ServiceRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VehicleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    TypeSetManually = table.Column<bool>(type: "bit", nullable: false),
                    Km = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Complaint = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    DeliveredAt = table.Column<DateOnly>(type: "date", nullable: true),
                    PaymentMethod = table.Column<int>(type: "int", nullable: true),
                    PaymentCollectedAt = table.Column<DateOnly>(type: "date", nullable: true),
                    ReminderDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ReminderSent = table.Column<bool>(type: "bit", nullable: false),
                    WaitingForPart = table.Column<bool>(type: "bit", nullable: false),
                    WaitingSince = table.Column<DateOnly>(type: "date", nullable: true),
                    ShowWaitingToCustomer = table.Column<bool>(type: "bit", nullable: false),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateTimeUpdated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeDeleted = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceRecords_Shops_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Shops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServiceRecords_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ServiceLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ServiceRecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Kind = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Cost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    SuppliedBy = table.Column<int>(type: "int", nullable: true),
                    PartBrand = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PartCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MatchStatus = table.Column<int>(type: "int", nullable: false),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateTimeUpdated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeDeleted = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceLines_ServiceRecords_ServiceRecordId",
                        column: x => x.ServiceRecordId,
                        principalTable: "ServiceRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ServiceLines_Shops_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Shops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceLines_ServiceRecordId",
                table: "ServiceLines",
                column: "ServiceRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceLines_TenantId",
                table: "ServiceLines",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRecords_TenantId",
                table: "ServiceRecords",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRecords_VehicleId",
                table: "ServiceRecords",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_TenantId",
                table: "Vehicles",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_TenantId_PlateNo",
                table: "Vehicles",
                columns: new[] { "TenantId", "PlateNo" },
                unique: true,
                filter: "[DateTimeDeleted] IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServiceLines");

            migrationBuilder.DropTable(
                name: "ServiceRecords");

            migrationBuilder.DropTable(
                name: "Vehicles");

            migrationBuilder.DropTable(
                name: "Shops");
        }
    }
}
