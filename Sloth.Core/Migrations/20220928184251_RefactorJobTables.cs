using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sloth.Core.Migrations
{
    public partial class RefactorJobTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.AddColumn<DateTime>(
                name: "ProcessedDate",
                table: "JobRecords",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalTransactions",
                table: "JobRecords",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TransactionBlobs",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IntegrationId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    TransactionId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    BlobId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionBlobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransactionBlobs_Blobs_BlobId",
                        column: x => x.BlobId,
                        principalTable: "Blobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TransactionBlobs_Integrations_IntegrationId",
                        column: x => x.IntegrationId,
                        principalTable: "Integrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TransactionBlobs_Transactions_TransactionId",
                        column: x => x.TransactionId,
                        principalTable: "Transactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JobRecords_EndedAt",
                table: "JobRecords",
                column: "EndedAt");

            migrationBuilder.CreateIndex(
                name: "IX_JobRecords_Name",
                table: "JobRecords",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_JobRecords_ProcessedDate",
                table: "JobRecords",
                column: "ProcessedDate");

            migrationBuilder.CreateIndex(
                name: "IX_JobRecords_StartedAt",
                table: "JobRecords",
                column: "StartedAt");

            migrationBuilder.CreateIndex(
                name: "IX_JobRecords_Status",
                table: "JobRecords",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionBlobs_BlobId",
                table: "TransactionBlobs",
                column: "BlobId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionBlobs_IntegrationId",
                table: "TransactionBlobs",
                column: "IntegrationId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionBlobs_TransactionId",
                table: "TransactionBlobs",
                column: "TransactionId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TransactionBlobs");

            migrationBuilder.DropIndex(
                name: "IX_JobRecords_EndedAt",
                table: "JobRecords");

            migrationBuilder.DropIndex(
                name: "IX_JobRecords_Name",
                table: "JobRecords");

            migrationBuilder.DropIndex(
                name: "IX_JobRecords_ProcessedDate",
                table: "JobRecords");

            migrationBuilder.DropIndex(
                name: "IX_JobRecords_StartedAt",
                table: "JobRecords");

            migrationBuilder.DropIndex(
                name: "IX_JobRecords_Status",
                table: "JobRecords");

            migrationBuilder.DropColumn(
                name: "ProcessedDate",
                table: "JobRecords");

            migrationBuilder.DropColumn(
                name: "TotalTransactions",
                table: "JobRecords");
        }
    }
}
