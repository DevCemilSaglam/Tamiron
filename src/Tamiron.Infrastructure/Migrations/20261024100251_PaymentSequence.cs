using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tamiron.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PaymentSequence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Sequence",
                table: "Payments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // K-078.5 · Var olan ödemelere yazılış sırası: kayıt içinde gün, sonra oluşturma anı.
            // (Payments göçü kayıt başına bir satır üretir; birden çok satır yalnız sonradan girilmiş olabilir.)
            migrationBuilder.Sql(@"
WITH s AS (SELECT Sequence, ROW_NUMBER() OVER (PARTITION BY ServiceRecordId ORDER BY PaidOn, DateTimeCreated, Id) AS n FROM Payments)
UPDATE s SET Sequence = n;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Sequence",
                table: "Payments");
        }
    }
}
