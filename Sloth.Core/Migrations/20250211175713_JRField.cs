using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sloth.Core.Migrations
{
    public partial class JRField : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SavedTransactionId",
                table: "JournalRequests",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SavedTransactionId",
                table: "JournalRequests");
        }
    }
}
