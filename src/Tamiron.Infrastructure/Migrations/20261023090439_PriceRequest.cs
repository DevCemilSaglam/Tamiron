using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tamiron.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PriceRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SelectedReplyId",
                table: "ServiceLines",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PriceRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ServiceRecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SupplierId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SentText = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateTimeUpdated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeDeleted = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriceRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PriceRequests_ServiceRecords_ServiceRecordId",
                        column: x => x.ServiceRecordId,
                        principalTable: "ServiceRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PriceRequests_Shops_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Shops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PriceRequests_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PriceReplies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PriceRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RawText = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    ParsedPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    ParsedBrand = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ParsedCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ReceivedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateTimeUpdated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeDeleted = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriceReplies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PriceReplies_PriceRequests_PriceRequestId",
                        column: x => x.PriceRequestId,
                        principalTable: "PriceRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PriceReplies_Shops_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Shops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PriceRequestLines",
                columns: table => new
                {
                    PriceRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ServiceLineId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriceRequestLines", x => new { x.PriceRequestId, x.ServiceLineId });
                    table.ForeignKey(
                        name: "FK_PriceRequestLines_PriceRequests_PriceRequestId",
                        column: x => x.PriceRequestId,
                        principalTable: "PriceRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PriceRequestLines_ServiceLines_ServiceLineId",
                        column: x => x.ServiceLineId,
                        principalTable: "ServiceLines",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceLines_SelectedReplyId",
                table: "ServiceLines",
                column: "SelectedReplyId");

            migrationBuilder.CreateIndex(
                name: "IX_PriceReplies_PriceRequestId",
                table: "PriceReplies",
                column: "PriceRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_PriceReplies_TenantId",
                table: "PriceReplies",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_PriceRequestLines_ServiceLineId",
                table: "PriceRequestLines",
                column: "ServiceLineId");

            migrationBuilder.CreateIndex(
                name: "IX_PriceRequests_ServiceRecordId",
                table: "PriceRequests",
                column: "ServiceRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_PriceRequests_SupplierId",
                table: "PriceRequests",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_PriceRequests_TenantId",
                table: "PriceRequests",
                column: "TenantId");

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceLines_PriceReplies_SelectedReplyId",
                table: "ServiceLines",
                column: "SelectedReplyId",
                principalTable: "PriceReplies",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServiceLines_PriceReplies_SelectedReplyId",
                table: "ServiceLines");

            migrationBuilder.DropTable(
                name: "PriceReplies");

            migrationBuilder.DropTable(
                name: "PriceRequestLines");

            migrationBuilder.DropTable(
                name: "PriceRequests");

            migrationBuilder.DropIndex(
                name: "IX_ServiceLines_SelectedReplyId",
                table: "ServiceLines");

            migrationBuilder.DropColumn(
                name: "SelectedReplyId",
                table: "ServiceLines");
        }
    }
}
