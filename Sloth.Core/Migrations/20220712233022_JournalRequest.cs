using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sloth.Core.Migrations
{
    public partial class JournalRequest : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "JournalRequestId",
                table: "Transactions",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "JournalRequests",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SourceId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JournalRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JournalRequests_Sources_SourceId",
                        column: x => x.SourceId,
                        principalTable: "Sources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_JournalRequestId",
                table: "Transactions",
                column: "JournalRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalRequests_SourceId",
                table: "JournalRequests",
                column: "SourceId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_JournalRequests_JournalRequestId",
                table: "Transactions",
                column: "JournalRequestId",
                principalTable: "JournalRequests",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_JournalRequests_JournalRequestId",
                table: "Transactions");

            migrationBuilder.DropTable(
                name: "JournalRequests");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_JournalRequestId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "JournalRequestId",
                table: "Transactions");
        }
    }
}
