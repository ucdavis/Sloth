using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Sloth.Core.Migrations
{
    public partial class createapikeys : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApiKeys",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Issued = table.Column<DateTime>(nullable: false),
                    Owner = table.Column<string>(nullable: true),
                    Revoked = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiKeys", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApiKeys");
        }
    }
}
