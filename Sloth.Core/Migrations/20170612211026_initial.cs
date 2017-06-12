using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Sloth.Core.Migrations
{
    public partial class initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Email = table.Column<string>(nullable: true),
                    Username = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ApiKeys",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Issued = table.Column<DateTime>(nullable: false),
                    Revoked = table.Column<DateTime>(nullable: true),
                    UserId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiKeys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApiKeys_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Scrubbers",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    BatchDate = table.Column<DateTime>(nullable: false),
                    BatchSequenceNumber = table.Column<int>(nullable: false),
                    CampusCode = table.Column<string>(maxLength: 2, nullable: false),
                    Chart = table.Column<string>(maxLength: 2, nullable: false),
                    ContactAddress = table.Column<string>(maxLength: 30, nullable: false),
                    ContactDepartment = table.Column<string>(maxLength: 30, nullable: false),
                    ContactEmail = table.Column<string>(maxLength: 40, nullable: false),
                    ContactPhone = table.Column<string>(maxLength: 10, nullable: false),
                    ContactUserId = table.Column<string>(maxLength: 8, nullable: false),
                    CreatorId = table.Column<string>(nullable: true),
                    OrganizationCode = table.Column<string>(maxLength: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Scrubbers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Scrubbers_Users_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Transaction",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Account = table.Column<string>(maxLength: 7, nullable: false),
                    Amount = table.Column<decimal>(nullable: false),
                    BalanceType = table.Column<string>(maxLength: 2, nullable: false),
                    Chart = table.Column<int>(maxLength: 2, nullable: false),
                    DebitCredit = table.Column<string>(nullable: false),
                    Description = table.Column<string>(maxLength: 40, nullable: false),
                    DocType = table.Column<string>(nullable: false),
                    DocumentNumber = table.Column<string>(maxLength: 14, nullable: false),
                    FiscalPeriod = table.Column<int>(nullable: false),
                    FiscalYear = table.Column<int>(nullable: false),
                    ObjectCode = table.Column<string>(maxLength: 4, nullable: false),
                    ObjectType = table.Column<string>(maxLength: 2, nullable: true),
                    OriginCode = table.Column<string>(maxLength: 2, nullable: false),
                    Project = table.Column<string>(maxLength: 10, nullable: true),
                    ReferenceId = table.Column<string>(maxLength: 8, nullable: true),
                    ScrubberId = table.Column<string>(nullable: true),
                    SequenceNumber = table.Column<int>(nullable: false),
                    SubAccount = table.Column<string>(maxLength: 5, nullable: true),
                    SubObjectCode = table.Column<string>(maxLength: 3, nullable: true),
                    TrackingNumber = table.Column<string>(maxLength: 10, nullable: true),
                    TransactionDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transaction", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transaction_Scrubbers_ScrubberId",
                        column: x => x.ScrubberId,
                        principalTable: "Scrubbers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApiKeys_UserId",
                table: "ApiKeys",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Scrubbers_CreatorId",
                table: "Scrubbers",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_ScrubberId",
                table: "Transaction",
                column: "ScrubberId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApiKeys");

            migrationBuilder.DropTable(
                name: "Transaction");

            migrationBuilder.DropTable(
                name: "Scrubbers");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
