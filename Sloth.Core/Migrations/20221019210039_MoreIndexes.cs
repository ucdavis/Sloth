using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sloth.Core.Migrations
{
    public partial class MoreIndexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Transactions",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ProcessorTrackingNumber",
                table: "Transactions",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MerchantTrackingNumber",
                table: "Transactions",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_KfsTrackingNumber",
                table: "Transactions",
                column: "KfsTrackingNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_MerchantTrackingNumber",
                table: "Transactions",
                column: "MerchantTrackingNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_ProcessorTrackingNumber",
                table: "Transactions",
                column: "ProcessorTrackingNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_Status",
                table: "Transactions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_TransactionDate",
                table: "Transactions",
                column: "TransactionDate");

            migrationBuilder.CreateIndex(
                name: "IX_Teams_Slug",
                table: "Teams",
                column: "Slug",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Transactions_KfsTrackingNumber",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_MerchantTrackingNumber",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_ProcessorTrackingNumber",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_Status",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_TransactionDate",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Teams_Slug",
                table: "Teams");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Transactions",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ProcessorTrackingNumber",
                table: "Transactions",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MerchantTrackingNumber",
                table: "Transactions",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128,
                oldNullable: true);
        }
    }
}
