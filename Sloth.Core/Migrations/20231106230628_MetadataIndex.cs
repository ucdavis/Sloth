using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sloth.Core.Migrations
{
    public partial class MetadataIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Value",
                table: "TransactionMetadata",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionMetadata_TransactionId_Name_Value",
                table: "TransactionMetadata",
                columns: new[] { "TransactionId", "Name", "Value" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TransactionMetadata_TransactionId_Name_Value",
                table: "TransactionMetadata");

            migrationBuilder.AlterColumn<string>(
                name: "Value",
                table: "TransactionMetadata",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
