using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tamiron.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TraceEvent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TraceEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClientAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Kind = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Data = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateTimeUpdated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeDeleted = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TraceEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TraceEvents_Shops_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Shops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TraceEvents_DateTimeCreated",
                table: "TraceEvents",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_TraceEvents_SessionId_DateTimeCreated",
                table: "TraceEvents",
                columns: new[] { "SessionId", "DateTimeCreated" });

            migrationBuilder.CreateIndex(
                name: "IX_TraceEvents_TenantId",
                table: "TraceEvents",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TraceEvents");
        }
    }
}
