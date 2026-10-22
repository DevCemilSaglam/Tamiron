using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tamiron.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class VatPlaceholders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OwnerType",
                table: "Vehicles",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DefaultVatRate",
                table: "Shops",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                // ⚠️ EF 0 üretmişti: VAR OLAN her dükkânın KDV varsayılanı %0 olurdu (testler
                // görmez — her test sıfırdan kurulan veritabanında koşar). KDV.md: 20.
                defaultValue: 20m);

            migrationBuilder.AddColumn<decimal>(
                name: "WithholdingRate",
                table: "ServiceRecords",
                type: "decimal(5,4)",
                precision: 5,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "VatRate",
                table: "ServiceLines",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OwnerType",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "DefaultVatRate",
                table: "Shops");

            migrationBuilder.DropColumn(
                name: "WithholdingRate",
                table: "ServiceRecords");

            migrationBuilder.DropColumn(
                name: "VatRate",
                table: "ServiceLines");
        }
    }
}
