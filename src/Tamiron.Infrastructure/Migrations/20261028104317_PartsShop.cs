using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tamiron.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PartsShop : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Kind",
                table: "Shops",
                type: "int",
                nullable: false,
                defaultValue: 1);   // var olan dükkânlar TAMİRHANE (ShopKind.Tamirhane = 1) — 0 geçersiz tür olurdu

            migrationBuilder.AddColumn<Guid>(
                name: "SupplierAnswerId",
                table: "PriceReplies",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SupplierAnswers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PriceRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ServiceLineId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Brand = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Code = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Note = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    AnsweredAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateTimeUpdated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeDeleted = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierAnswers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupplierAnswers_Shops_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Shops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PriceReplies_SupplierAnswerId",
                table: "PriceReplies",
                column: "SupplierAnswerId",
                filter: "[SupplierAnswerId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierAnswers_PriceRequestId",
                table: "SupplierAnswers",
                column: "PriceRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierAnswers_TenantId",
                table: "SupplierAnswers",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SupplierAnswers");

            migrationBuilder.DropIndex(
                name: "IX_PriceReplies_SupplierAnswerId",
                table: "PriceReplies");

            migrationBuilder.DropColumn(
                name: "Kind",
                table: "Shops");

            migrationBuilder.DropColumn(
                name: "SupplierAnswerId",
                table: "PriceReplies");
        }
    }
}
