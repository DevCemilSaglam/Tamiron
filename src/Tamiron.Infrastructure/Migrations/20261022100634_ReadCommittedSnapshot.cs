using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tamiron.Infrastructure.Migrations
{
    /// <summary>
    /// MIMARI §4.1: "Bir yazma kilidi okumayı ENGELLEMEZ." Okumalar son işlenmiş
    /// sürümü görür, yazanı beklemez. ALTER DATABASE işlem (transaction) içinde
    /// koşamaz — o yüzden suppressTransaction.
    /// </summary>
    public partial class ReadCommittedSnapshot : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
            => migrationBuilder.Sql("ALTER DATABASE CURRENT SET READ_COMMITTED_SNAPSHOT ON WITH ROLLBACK IMMEDIATE;",
                                    suppressTransaction: true);

        protected override void Down(MigrationBuilder migrationBuilder)
            => migrationBuilder.Sql("ALTER DATABASE CURRENT SET READ_COMMITTED_SNAPSHOT OFF WITH ROLLBACK IMMEDIATE;",
                                    suppressTransaction: true);
    }
}
